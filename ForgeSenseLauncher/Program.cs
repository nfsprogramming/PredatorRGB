using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace ForgeSenseLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Process? coreProcess = null;
            string tempDir = "";
            bool isInstalled = false;
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string localCore = Path.Combine(appDir, "ForgeSense.Core.exe");
                string localUi = Path.Combine(appDir, "PredatorRGB.exe");
                if (!File.Exists(localUi)) localUi = Path.Combine(appDir, "forgesense_ui.exe");

                string runDir;
                if (File.Exists(localCore) && File.Exists(localUi))
                {
                    isInstalled = true;
                    runDir = appDir;
                }
                else
                {
                    tempDir = Path.Combine(Path.GetTempPath(), "ForgeSense_" + Guid.NewGuid().ToString().Substring(0, 8));
                    Directory.CreateDirectory(tempDir);

                    var assembly = Assembly.GetExecutingAssembly();
                    using (Stream stream = assembly.GetManifestResourceStream("ForgeSenseLauncher.ForgeSense_Windows_v1.zip"))
                    {
                        if (stream != null)
                        {
                            using (ZipArchive archive = new ZipArchive(stream))
                            {
                                archive.ExtractToDirectory(tempDir);
                            }
                        }
                        else
                        {
                            throw new Exception("Resource ForgeSense_Windows_v1.zip not found in assembly!");
                        }
                    }
                    runDir = tempDir;
                }

                // 1. Launch Backend Process (ForgeSense.Core.exe) hidden in background
                string coreExe = Path.Combine(runDir, "ForgeSense.Core.exe");
                if (File.Exists(coreExe))
                {
                    var coreStartInfo = new ProcessStartInfo
                    {
                        FileName = coreExe,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = runDir
                    };
                    coreProcess = Process.Start(coreStartInfo);
                    Thread.Sleep(800); // Allow backend to bind TCP port 4050
                }

                // 2. Launch UI Process (PredatorRGB.exe or forgesense_ui.exe)
                string uiExe = Path.Combine(runDir, "PredatorRGB.exe");
                if (!File.Exists(uiExe)) uiExe = Path.Combine(runDir, "forgesense_ui.exe");

                var uiStartInfo = new ProcessStartInfo
                {
                    FileName = uiExe,
                    UseShellExecute = true,
                    WorkingDirectory = runDir
                };

                using (var uiProcess = Process.Start(uiStartInfo))
                {
                    uiProcess?.WaitForExit();
                }

                // 3. Gracefully stop backend on UI exit
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    var ar = client.BeginConnect(System.Net.IPAddress.Loopback, 4050, null, null);
                    if (ar.AsyncWaitHandle.WaitOne(1000))
                    {
                        client.EndConnect(ar);
                        using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                        writer.WriteLine("EXIT");
                    }
                }
                catch { }

                try
                {
                    if (coreProcess != null && !coreProcess.HasExited)
                    {
                        coreProcess.Kill();
                        coreProcess.WaitForExit(1000);
                    }
                }
                catch { }

                // Cleanup temp after exit if portable run
                if (!isInstalled && !string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }
            }
            catch (Exception ex)
            {
                try { File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher_error.log"), ex.ToString()); } catch { }
            }
            finally
            {
                try
                {
                    if (coreProcess != null && !coreProcess.HasExited)
                    {
                        coreProcess.Kill();
                    }
                }
                catch { }
            }
        }
    }
}
