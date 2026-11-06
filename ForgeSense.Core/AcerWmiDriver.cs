using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.Versioning;

namespace ForgeSense.Core
{
    [SupportedOSPlatform("windows")]
    public class AcerWmiDriver : IDisposable
    {
        private ManagementObject? _wmiObject;
        private ManagementBaseObject? _cachedInParamsRgb;
        private bool _isInitialized = false;
        private readonly object _lock = new();
        private readonly uint[] _zoneIds = new uint[] { 1, 2, 4, 8 }; // Powers of 2 for SetGamingRgbKb

        public bool IsAvailable => _isInitialized && _wmiObject != null;

        private void Log(string message)
        {
            try
            {
                string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ForgeSense", "Logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "forgesense_hardware.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\r\n");
            }
            catch { }
            Console.WriteLine("[AcerWmiDriver] " + message);
        }

        public bool Initialize()
        {
            lock (_lock)
            {
                try
                {
                    Log("Searching for AcerGamingFunction in root\\wmi...");
                    var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM AcerGamingFunction");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        _wmiObject = obj;
                        break;
                    }

                    if (_wmiObject == null)
                    {
                        Log("AcerGamingFunction NOT found in root\\wmi.");
                        return false;
                    }

                    Log("Found AcerGamingFunction! Initializing keyboard hardware...");

                    // 1. GetGamingSysInfo
                    try
                    {
                        var inParams0 = _wmiObject.GetMethodParameters("GetGamingSysInfo");
                        inParams0["gmInput"] = (uint)0;
                        _wmiObject.InvokeMethod("GetGamingSysInfo", inParams0, null);
                        Log("Step 1: GetGamingSysInfo succeeded.");
                    }
                    catch (Exception ex)
                    {
                        Log("Step 1 GetGamingSysInfo warning: " + ex.Message);
                    }

                    // 2. SetGamingKBBacklight: Force Static Mode (Effect 0), 100% Brightness to seize control from internal EC animation
                    try
                    {
                        var inParamsKb = _wmiObject.GetMethodParameters("SetGamingKBBacklight");
                        byte[] kbPayload = new byte[16];
                        kbPayload[0] = 0;   // Leading WMI prefix
                        kbPayload[1] = 0;   // Effect ID 0 = Static
                        kbPayload[2] = 0;   // Speed 0
                        kbPayload[3] = 100; // Brightness 100%
                        kbPayload[10] = 1;  // Apply flag
                        inParamsKb["gmInput"] = kbPayload;
                        _wmiObject.InvokeMethod("SetGamingKBBacklight", inParamsKb, null);
                        Log("Step 2: SetGamingKBBacklight (Static Mode, 100% brightness) seized control from EC animation.");
                    }
                    catch (Exception ex)
                    {
                        Log("Step 2 SetGamingKBBacklight warning: " + ex.Message);
                    }

                    // 3. SetGamingLED: Turn on all 4 Zones (Mode 8, Zones 15 = 1111 binary)
                    try
                    {
                        var inParams1 = _wmiObject.GetMethodParameters("SetGamingLED");
                        byte[] ledInitPayload = new byte[8];
                        ledInitPayload[0] = 8;
                        ledInitPayload[5] = 15;
                        inParams1["gmInput"] = ledInitPayload;
                        _wmiObject.InvokeMethod("SetGamingLED", inParams1, null);
                        Log("Step 3: SetGamingLED (4 zones enabled).");
                    }
                    catch (Exception ex)
                    {
                        Log("Step 3 SetGamingLED warning: " + ex.Message);
                    }

                    _isInitialized = true;
                    Log("Acer Predator 4-Zone WMI hardware initialized successfully!");
                    return true;
                }
                catch (Exception ex)
                {
                    Log("Initialization FAILED: " + ex.Message);
                    _isInitialized = false;
                    return false;
                }
            }
        }

        public void SetBrightness(float brightness)
        {
            if (!_isInitialized || _wmiObject == null) return;
            lock (_lock)
            {
                try
                {
                    byte bVal = (byte)Math.Clamp((int)(brightness * 100), 0, 100);
                    var inParamsKb = _wmiObject.GetMethodParameters("SetGamingKBBacklight");
                    byte[] kbPayload = new byte[16];
                    kbPayload[0] = 0;
                    kbPayload[1] = 0; // Static mode
                    kbPayload[2] = 0;
                    kbPayload[3] = bVal; // Brightness
                    kbPayload[10] = 1;
                    inParamsKb["gmInput"] = kbPayload;
                    _wmiObject.InvokeMethod("SetGamingKBBacklight", inParamsKb, null);
                }
                catch (Exception ex)
                {
                    Log("SetBrightness error: " + ex.Message);
                }
            }
        }

        public byte CurrentNativeEffect { get; private set; } = 0;

        public void SetNativeEffect(byte effectId, byte speed = 5, byte brightness = 100, byte direction = 0, byte r1 = 0, byte g1 = 0, byte b1 = 0)
        {
            if (!_isInitialized || _wmiObject == null) return;
            lock (_lock)
            {
                try
                {
                    CurrentNativeEffect = effectId;
                    var inParamsKb = _wmiObject.GetMethodParameters("SetGamingKBBacklight");
                    byte[] kbPayload = new byte[16];
                    kbPayload[0] = 0;          // Prefix
                    kbPayload[1] = effectId;   // 0=Static, 1=Breathing, 2=Neon/Spectrum, 3=Wave, 4=Zoom, 5=Shifting
                    kbPayload[2] = speed;      // Speed (0-9)
                    kbPayload[3] = brightness; // Brightness (0-100)
                    kbPayload[4] = direction;  // Direction (0=Right/Down, 1=Left/Up)
                    kbPayload[5] = r1;         // Primary Red
                    kbPayload[6] = g1;         // Primary Green
                    kbPayload[7] = b1;         // Primary Blue
                    kbPayload[8] = 3;          // Mode byte
                    kbPayload[9] = 1;          // Mode byte
                    kbPayload[10] = 1;         // Apply flag
                    inParamsKb["gmInput"] = kbPayload;
                    _wmiObject.InvokeMethod("SetGamingKBBacklight", inParamsKb, null);
                    Log($"SetNativeEffect: effectId={effectId}, speed={speed}, brightness={brightness}, color=({r1},{g1},{b1})");
                }
                catch (Exception ex)
                {
                    Log("SetNativeEffect error: " + ex.Message);
                }
            }
        }

        private readonly byte[] _lastAppliedR = new byte[4];
        private readonly byte[] _lastAppliedG = new byte[4];
        private readonly byte[] _lastAppliedB = new byte[4];
        private bool _hasAppliedColors = false;

        public void SetAllZones(byte[] r, byte[] g, byte[] b)
        {
            if (!_isInitialized || _wmiObject == null) return;

            lock (_lock)
            {
                int count = Math.Min(4, Math.Min(r.Length, Math.Min(g.Length, b.Length)));
                if (count < 4) return;

                // 1. Uniform Color Fast-Path (Breathing, Pulse, Static, Uniform Fades)
                // When all 4 zones share the identical color, send one single WMI call with mask 15 (0b1111)
                if (r[0] == r[1] && r[1] == r[2] && r[2] == r[3] &&
                    g[0] == g[1] && g[1] == g[2] && g[2] == g[3] &&
                    b[0] == b[1] && b[1] == b[2] && b[2] == b[3])
                {
                    if (_hasAppliedColors &&
                        Math.Abs(r[0] - _lastAppliedR[0]) < 2 &&
                        Math.Abs(g[0] - _lastAppliedG[0]) < 2 &&
                        Math.Abs(b[0] - _lastAppliedB[0]) < 2)
                    {
                        return;
                    }

                    try
                    {
                        uint allZonesMask = 15; // Zone 1 | 2 | 4 | 8 = 15
                        ulong rgbKbPayload = ((ulong)b[0] << 24) | ((ulong)g[0] << 16) | ((ulong)r[0] << 8) | (ulong)allZonesMask;
                        _cachedInParamsRgb ??= _wmiObject.GetMethodParameters("SetGamingRgbKb");
                        _cachedInParamsRgb["gmInput"] = rgbKbPayload;
                        _wmiObject.InvokeMethod("SetGamingRgbKb", _cachedInParamsRgb, null);

                        for (int z = 0; z < 4; z++)
                        {
                            _lastAppliedR[z] = r[0];
                            _lastAppliedG[z] = g[0];
                            _lastAppliedB[z] = b[0];
                        }
                        _hasAppliedColors = true;
                        return;
                    }
                    catch { }
                }

                // 2. Multi-Zone Grouping by identical/near-identical color
                // Groups zones with identical RGB into a single WMI call with combined bitmask!
                bool[] zoneHandled = new bool[4];
                for (int i = 0; i < 4; i++)
                {
                    if (zoneHandled[i]) continue;

                    // If color hasn't changed noticeably (delta threshold < 3), skip to save ACPI bandwidth
                    if (_hasAppliedColors &&
                        Math.Abs(r[i] - _lastAppliedR[i]) < 3 &&
                        Math.Abs(g[i] - _lastAppliedG[i]) < 3 &&
                        Math.Abs(b[i] - _lastAppliedB[i]) < 3)
                    {
                        zoneHandled[i] = true;
                        continue;
                    }

                    uint mask = _zoneIds[i];
                    zoneHandled[i] = true;

                    // Group other zones sharing the exact same color
                    for (int j = i + 1; j < 4; j++)
                    {
                        if (!zoneHandled[j] && r[j] == r[i] && g[j] == g[i] && b[j] == b[i])
                        {
                            mask |= _zoneIds[j];
                            zoneHandled[j] = true;
                        }
                    }

                    try
                    {
                        ulong rgbKbPayload = ((ulong)b[i] << 24) | ((ulong)g[i] << 16) | ((ulong)r[i] << 8) | (ulong)mask;
                        _cachedInParamsRgb ??= _wmiObject.GetMethodParameters("SetGamingRgbKb");
                        _cachedInParamsRgb["gmInput"] = rgbKbPayload;
                        _wmiObject.InvokeMethod("SetGamingRgbKb", _cachedInParamsRgb, null);
                    }
                    catch { }

                    for (int z = 0; z < 4; z++)
                    {
                        if ((mask & _zoneIds[z]) != 0)
                        {
                            _lastAppliedR[z] = r[i];
                            _lastAppliedG[z] = g[i];
                            _lastAppliedB[z] = b[i];
                        }
                    }
                }
                _hasAppliedColors = true;
            }
        }

        public void SetZoneColor(uint zoneIndex, byte r, byte g, byte b)
        {
            if (!_isInitialized || _wmiObject == null || zoneIndex < 1 || zoneIndex > 4) return;

            lock (_lock)
            {
                int idx = (int)(zoneIndex - 1);
                uint zoneId = _zoneIds[idx];
                bool primarySucceeded = false;

                try
                {
                    ulong rgbKbPayload = ((ulong)b << 24) | ((ulong)g << 16) | ((ulong)r << 8) | (ulong)zoneId;
                    var inParamsRgb = _wmiObject.GetMethodParameters("SetGamingRgbKb");
                    inParamsRgb["gmInput"] = rgbKbPayload;
                    _wmiObject.InvokeMethod("SetGamingRgbKb", inParamsRgb, null);
                    primarySucceeded = true;
                }
                catch { }

                if (!primarySucceeded)
                {
                    try
                    {
                        ulong ledColorPayload = (ulong)zoneIndex | ((ulong)r << 8) | ((ulong)g << 16) | ((ulong)b << 24);
                        var inParamsCol = _wmiObject.GetMethodParameters("SetGamingLEDColor");
                        inParamsCol["gmInput"] = ledColorPayload;
                        _wmiObject.InvokeMethod("SetGamingLEDColor", inParamsCol, null);
                    }
                    catch { }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _cachedInParamsRgb?.Dispose();
                _cachedInParamsRgb = null;
                _wmiObject?.Dispose();
                _wmiObject = null;
                _isInitialized = false;
            }
        }
    }
}
