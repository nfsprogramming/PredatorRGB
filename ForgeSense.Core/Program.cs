using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ForgeSense.Core
{
    class Program
    {
        private static Mutex? _singleInstanceMutex;
        private static readonly ManualResetEventSlim _exitEvent = new(false);

        public static void Log(string msg)
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ForgeSense", "Logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "forgesense_core.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {msg}\r\n");
            }
            catch { }
            Console.WriteLine(msg);
        }

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log("FATAL UNHANDLED EXCEPTION: " + e.ExceptionObject);
            };

            bool createdNew;
            _singleInstanceMutex = new Mutex(true, "ForgeSenseCoreEngineMutex", out createdNew);

            if (!createdNew)
            {
                Log("Another instance of ForgeSense Core Engine is already running. Exiting.");
                return;
            }

            Log("================================================");
            Log("Starting ForgeSense Core Engine (Daemon Mode)...");
            Log("Base Directory: " + AppDomain.CurrentDomain.BaseDirectory);
            Log("Operating System: " + Environment.OSVersion);
            Log("================================================");

            // Automatic handling of OEM Acer Lighting Service
            try
            {
                string acerStatus = WindowsServiceManager.GetServiceStatus(WindowsServiceManager.AcerLightingServiceName);
                Log($"[Startup] AcerLightingService status: {acerStatus}");
                if (acerStatus.Equals("RUNNING", StringComparison.OrdinalIgnoreCase))
                {
                    Log("[Startup] Stopping conflicting AcerLightingService to give PredatorRGB exclusive hardware control...");
                    var stopResult = WindowsServiceManager.StopService(WindowsServiceManager.AcerLightingServiceName);
                    Log($"[Startup] AcerLightingService stop result: {stopResult.Success} - {stopResult.Message}");
                }
            }
            catch (Exception ex)
            {
                Log("[Startup] AcerLightingService check warning: " + ex.Message);
            }

            try
            {
                var telemetry = new TelemetryEngine();
                var reactive = new ReactiveEngine();
                var audio = new ForgeSense.Core.DSP.AudioAnalyzer();

                // Set default audio source
                audio.SetAudioSource(ForgeSense.Core.DSP.AudioSourceType.SystemAudio);

                using var lighting = new LightingEngine(telemetry, reactive, audio);
                using var screenSync = new ScreenSyncEngine(lighting);

                telemetry.Start();
                lighting.Start();
                screenSync.Start();

                Log($"Engines started successfully! Hardware: {(lighting.HasAcerWmi ? "Acer Predator WMI 4-Zone" : "Simulation")}");
                Log("Starting RPC TCP Server on 127.0.0.1:4050...");

                // Background tasks for IPC & Telemetry
                var rpcTask = Task.Run(() => StartRpcServer(lighting));
                var udpTask = Task.Run(() => StartTelemetryStream(audio));

                // Wait indefinitely until an explicit EXIT command is received
                _exitEvent.Wait();

                Log("Exit signal received. Shutting down ForgeSense Core gracefully...");
            }
            catch (Exception ex)
            {
                Log("Error during Core execution: " + ex);
            }
            finally
            {
                _singleInstanceMutex?.ReleaseMutex();
                _singleInstanceMutex?.Dispose();
                Log("ForgeSense Core stopped.");
            }
        }

        static async Task StartRpcServer(LightingEngine lighting)
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 4050);
            try
            {
                listener.Start();
                Log("RPC Server listening on 127.0.0.1:4050.");
            }
            catch (Exception ex)
            {
                Log("Failed to start RPC server: " + ex.Message);
                return;
            }

            while (!_exitEvent.IsSet)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client, lighting));
                }
                catch (Exception ex)
                {
                    if (!_exitEvent.IsSet)
                        Log("RPC Accept error: " + ex.Message);
                }
            }
        }

        static async Task HandleClientAsync(System.Net.Sockets.TcpClient client, LightingEngine lighting)
        {
            try
            {
                client.NoDelay = true;
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new System.IO.StreamReader(stream))
                using (var writer = new System.IO.StreamWriter(stream) { AutoFlush = true })
                {
                    while (client.Connected && !_exitEvent.IsSet)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null) break;

                        if (!line.StartsWith("ZONES:", StringComparison.OrdinalIgnoreCase) &&
                            !line.StartsWith("PING", StringComparison.OrdinalIgnoreCase))
                        {
                            Log("Received command: " + line);
                        }

                        if (line.StartsWith("ZONES:", StringComparison.OrdinalIgnoreCase))
                        {
                            // format: ZONES:r0,g0,b0:r1,g1,b1:r2,g2,b2:r3,g3,b3
                            string payload = line.Substring(6);
                            var zones = payload.Split(':');
                            if (zones.Length >= 4)
                            {
                                byte[] r = new byte[4];
                                byte[] g = new byte[4];
                                byte[] b = new byte[4];
                                bool valid = true;
                                for (int i = 0; i < 4; i++)
                                {
                                    var rgb = zones[i].Split(',');
                                    if (rgb.Length == 3 &&
                                        byte.TryParse(rgb[0], out r[i]) &&
                                        byte.TryParse(rgb[1], out g[i]) &&
                                        byte.TryParse(rgb[2], out b[i]))
                                    {
                                    }
                                    else
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                                if (valid)
                                {
                                    lighting.UpdateLiveZones(r, g, b);
                                }
                            }
                        }
                        else if (line.StartsWith("MODE:", StringComparison.OrdinalIgnoreCase))
                        {
                            string modeStr = line.Replace("MODE:", "").ToLowerInvariant();
                            lighting.CommandQueue.Enqueue(() =>
                            {
                                lighting.Manager.SetEffect(modeStr);

                                // If Acer EC WMI is available, route native modes directly to hardware
                                if (lighting.AcerDriver != null && lighting.AcerDriver.IsAvailable)
                                {
                                    string norm = modeStr.Replace("_", "").Replace("-", "").Replace(" ", "");
                                    byte speedVal = (byte)Math.Clamp((int)(lighting.Manager.State.Speed * 5), 1, 9);
                                    byte brightVal = (byte)Math.Clamp((int)(lighting.Manager.State.Brightness * 100), 0, 100);
                                    byte r = (byte)lighting.Manager.State.Color.R;
                                    byte g = (byte)lighting.Manager.State.Color.G;
                                    byte b = (byte)lighting.Manager.State.Color.B;

                                    if (norm.Contains("wave") || norm.Contains("radar") || norm.Contains("scanner"))
                                    {
                                        lighting.AcerDriver.SetNativeEffect(3, speedVal, brightVal, 0, r, g, b); // Mode 3 = Wave (Hardware EC)
                                    }
                                    else if (norm.Contains("breath") || norm.Contains("fade") || norm.Contains("pulse"))
                                    {
                                        lighting.AcerDriver.SetNativeEffect(1, speedVal, brightVal, 0, r, g, b); // Mode 1 = Breathing (Hardware EC)
                                    }
                                    else if (norm.Contains("neon") || norm.Contains("spectrum") || norm.Contains("cycle"))
                                    {
                                        lighting.AcerDriver.SetNativeEffect(2, speedVal, brightVal, 0, r, g, b); // Mode 2 = Neon (Hardware EC)
                                    }
                                    else if (norm.Contains("shift") || norm.Contains("chase"))
                                    {
                                        lighting.AcerDriver.SetNativeEffect(5, speedVal, brightVal, 0, r, g, b); // Mode 5 = Shifting (Hardware EC)
                                    }
                                    else if (norm.Contains("zoom") || norm.Contains("centerout"))
                                    {
                                        lighting.AcerDriver.SetNativeEffect(4, speedVal, brightVal, 0, r, g, b); // Mode 4 = Zoom (Hardware EC)
                                    }
                                    else
                                    {
                                        lighting.AcerDriver.SetNativeEffect(0, speedVal, brightVal, 0, r, g, b); // Mode 0 = Direct / Static for live zone stream
                                    }
                                }
                            });
                            writer.WriteLine("OK");
                        }
                        else if (line.StartsWith("COLOR:", StringComparison.OrdinalIgnoreCase))
                        {
                            // format: COLOR:R,G,B:VERSION
                            string payload = line.Substring(6);
                            var parts = payload.Split(':');
                            if (parts.Length == 2)
                            {
                                var rgb = parts[0].Split(',');
                                if (rgb.Length == 3 &&
                                    byte.TryParse(rgb[0], out byte r) &&
                                    byte.TryParse(rgb[1], out byte g) &&
                                    byte.TryParse(rgb[2], out byte b) &&
                                    int.TryParse(parts[1], out int version))
                                {
                                    var c = new OpenRGB.NET.Color(r, g, b);
                                    lighting.CommandQueue.Enqueue(() =>
                                    {
                                        lighting.Manager.SetColor(c, version);
                                        if (lighting.AcerDriver != null && lighting.AcerDriver.IsAvailable)
                                        {
                                            if (lighting.AcerDriver.CurrentNativeEffect > 0)
                                            {
                                                byte speedVal = (byte)Math.Clamp((int)(lighting.Manager.State.Speed * 5), 1, 9);
                                                byte brightVal = (byte)Math.Clamp((int)(lighting.Manager.State.Brightness * 100), 0, 100);
                                                lighting.AcerDriver.SetNativeEffect(lighting.AcerDriver.CurrentNativeEffect, speedVal, brightVal, 0, r, g, b);
                                            }
                                        }
                                    });
                                    writer.WriteLine("OK");
                                }
                                else writer.WriteLine("ERROR: Invalid Color Format");
                            }
                            else writer.WriteLine("ERROR: Invalid Color Payload");
                        }
                        else if (line.StartsWith("BRIGHTNESS:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (float.TryParse(line.Substring(11), out float b))
                            {
                                lighting.Manager.SetBrightness(b);
                                lighting.AcerDriver?.SetBrightness(b);
                                if (lighting.AcerDriver != null && lighting.AcerDriver.IsAvailable && lighting.AcerDriver.CurrentNativeEffect > 0)
                                {
                                    byte speedVal = (byte)Math.Clamp((int)(lighting.Manager.State.Speed * 5), 1, 9);
                                    byte brightVal = (byte)Math.Clamp((int)(b * 100), 0, 100);
                                    lighting.AcerDriver.SetNativeEffect(lighting.AcerDriver.CurrentNativeEffect, speedVal, brightVal);
                                }
                            }
                            writer.WriteLine("OK");
                        }
                        else if (line.StartsWith("SPEED:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (float.TryParse(line.Substring(6), out float s))
                            {
                                lighting.Manager.SetSpeed(s);
                                if (lighting.AcerDriver != null && lighting.AcerDriver.IsAvailable && lighting.AcerDriver.CurrentNativeEffect > 0)
                                {
                                    byte speedVal = (byte)Math.Clamp((int)(s * 5), 1, 9);
                                    byte brightVal = (byte)Math.Clamp((int)(lighting.Manager.State.Brightness * 100), 0, 100);
                                    lighting.AcerDriver.SetNativeEffect(lighting.AcerDriver.CurrentNativeEffect, speedVal, brightVal);
                                }
                            }
                            writer.WriteLine("OK");
                        }
                        else if (line.StartsWith("RESET", StringComparison.OrdinalIgnoreCase))
                        {
                            lighting.Manager.Reset();
                            lighting.Manager.SetEffect("static");
                            writer.WriteLine("OK");
                        }
                        else if (line.StartsWith("SERVICE:ACER:STATUS", StringComparison.OrdinalIgnoreCase))
                        {
                            string status = WindowsServiceManager.GetServiceStatus(WindowsServiceManager.AcerLightingServiceName);
                            Log("Acer service status check: " + status);
                            writer.WriteLine("STATUS:ACER:" + status);
                        }
                        else if (line.StartsWith("SERVICE:ACER:STOP", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = WindowsServiceManager.StopService(WindowsServiceManager.AcerLightingServiceName);
                            Log("Acer service stop requested: " + result.Message);
                            writer.WriteLine(result.Success ? "OK:" + result.Message : "ERROR:" + result.Message);
                        }
                        else if (line.StartsWith("SERVICE:ACER:START", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = WindowsServiceManager.StartService(WindowsServiceManager.AcerLightingServiceName);
                            Log("Acer service start requested: " + result.Message);
                            writer.WriteLine(result.Success ? "OK:" + result.Message : "ERROR:" + result.Message);
                        }
                        else if (line.StartsWith("HARDWARE:STATUS", StringComparison.OrdinalIgnoreCase))
                        {
                            string hw = lighting.HasAcerWmi ? "ACER_WMI_4ZONE" : "SIMULATION";
                            writer.WriteLine("OK:HARDWARE:" + hw);
                        }
                        else if (line.StartsWith("PING", StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteLine("PONG");
                        }
                        else if (line.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase))
                        {
                            writer.WriteLine("OK");
                            _exitEvent.Set();
                            break;
                        }
                        else
                        {
                            writer.WriteLine("ERROR: Unknown Command");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("RPC Client Connection Error: " + ex.Message);
            }
        }

        static async Task StartTelemetryStream(ForgeSense.Core.DSP.AudioAnalyzer audio)
        {
            var udp = new System.Net.Sockets.UdpClient();
            var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 4051);
            while (!_exitEvent.IsSet)
            {
                try
                {
                    string json = "";
                    lock (audio.Features)
                    {
                        json = System.Text.Json.JsonSerializer.Serialize(audio.Features);
                    }
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                    await udp.SendAsync(data, data.Length, endpoint);
                }
                catch { }
                await Task.Delay(50); // 20Hz
            }
        }
    }
}
