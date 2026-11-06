using System;
using System.Management;

namespace WmiTester
{
    class Program
    {
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Acer Keyboard Lighting via WMI...");
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM AcerGamingFunction");
                foreach (ManagementObject obj in searcher.Get())
                {
                    // 1. GetGamingSysInfo
                    try
                    {
                        var p0 = obj.GetMethodParameters("GetGamingSysInfo");
                        p0["gmInput"] = (uint)0;
                        obj.InvokeMethod("GetGamingSysInfo", p0, null);
                        Console.WriteLine("[1] GetGamingSysInfo OK");
                    }
                    catch (Exception e) { Console.WriteLine("[1] GetGamingSysInfo Error: " + e.Message); }

                    // 2. SetGamingKBBacklight to Static (Effect ID 0) to seize control from EC dynamic animations
                    try
                    {
                        var pKb = obj.GetMethodParameters("SetGamingKBBacklight");
                        byte[] kbPayload = new byte[16];
                        kbPayload[0] = 0;   // prefix
                        kbPayload[1] = 0;   // effect 0 = static
                        kbPayload[2] = 0;   // speed = 0
                        kbPayload[3] = 100; // brightness 100%
                        kbPayload[10] = 1;  // apply flag
                        pKb["gmInput"] = kbPayload;
                        obj.InvokeMethod("SetGamingKBBacklight", pKb, null);
                        Console.WriteLine("[2] SetGamingKBBacklight (Static Mode 100% brightness) OK");
                    }
                    catch (Exception e) { Console.WriteLine("[2] SetGamingKBBacklight Error: " + e.Message); }

                    // 3. SetGamingLED (Enable 4 zones)
                    try
                    {
                        var pLed = obj.GetMethodParameters("SetGamingLED");
                        byte[] ledPayload = new byte[8];
                        ledPayload[0] = 8;
                        ledPayload[5] = 15;
                        pLed["gmInput"] = ledPayload;
                        obj.InvokeMethod("SetGamingLED", pLed, null);
                        Console.WriteLine("[3] SetGamingLED (4 Zones Enabled) OK");
                    }
                    catch (Exception e) { Console.WriteLine("[3] SetGamingLED Error: " + e.Message); }

                    // 4. SetGamingRgbKb (Powers of 2: Zone 1=1, Zone 2=2, Zone 3=4, Zone 4=8)
                    uint[] zones = new uint[] { 1, 2, 4, 8 };
                    // Test: Set all 4 zones to Red (R=255, G=0, B=0)
                    byte r = 255, g = 0, b = 0;
                    for (int i = 0; i < zones.Length; i++)
                    {
                        uint zoneId = zones[i];
                        ulong payload = ((ulong)b << 24) | ((ulong)g << 16) | ((ulong)r << 8) | (ulong)zoneId;
                        try
                        {
                            var pRgb = obj.GetMethodParameters("SetGamingRgbKb");
                            pRgb["gmInput"] = payload;
                            obj.InvokeMethod("SetGamingRgbKb", pRgb, null);
                            Console.WriteLine($"[4] SetGamingRgbKb Zone {i + 1} (ID {zoneId}) -> RED OK");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[4] SetGamingRgbKb Zone {i + 1} Error: " + e.Message);
                        }

                        // Also send SetGamingLEDColor as fallback
                        try
                        {
                            ulong fallbackPayload = (ulong)(uint)(i + 1) | ((ulong)r << 8) | ((ulong)g << 16) | ((ulong)b << 24);
                            var pCol = obj.GetMethodParameters("SetGamingLEDColor");
                            pCol["gmInput"] = fallbackPayload;
                            obj.InvokeMethod("SetGamingLEDColor", pCol, null);
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.Message);
            }
        }
    }
}
