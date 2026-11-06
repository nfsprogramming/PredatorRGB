using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ForgeSense.Core
{
    public class ReactiveEngine : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Tracking state for Phase 1 Reactive Zone
        public long[] LastReleaseTimes { get; } = new long[4] { 0, 0, 0, 0 };
        public bool[] IsZoneHeld { get; } = new bool[4] { false, false, false, false };
        public int ActiveZone { get; private set; } = -1;
        
        private int[] _keysHeldInZone = new int[4];
        
        private Thread _pollThread;
        private bool _isRunning = true;
        private bool[] _wasPressed = new bool[256];
        private bool _isEffectActive = false;

        public ReactiveEngine()
        {
            _pollThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    for (int i = 1; i < 255; i++)
                    {
                        short state = GetAsyncKeyState(i);
                        bool isPressed = (state & 0x8000) != 0;
                        
                        if (_isEffectActive)
                        {
                            if (isPressed && !_wasPressed[i])
                            {
                                int zone = GetZoneForKey(i);
                                if (zone >= 0 && zone < 4)
                                {
                                    _keysHeldInZone[zone]++;
                                    ActiveZone = zone;
                                    IsZoneHeld[zone] = true;
                                    Console.WriteLine($"Reactive Trigger: Key {i} pressed in Zone {zone}");
                                }
                            }
                            else if (!isPressed && _wasPressed[i])
                            {
                                int zone = GetZoneForKey(i);
                                if (zone >= 0 && zone < 4)
                                {
                                    _keysHeldInZone[zone] = Math.Max(0, _keysHeldInZone[zone] - 1);
                                    if (_keysHeldInZone[zone] == 0)
                                    {
                                        IsZoneHeld[zone] = false;
                                        if (ActiveZone == zone)
                                        {
                                            LastReleaseTimes[zone] = Environment.TickCount64;
                                        }
                                    }
                                }
                            }
                        }
                        _wasPressed[i] = isPressed;
                    }
                    Thread.Sleep(10); // 10ms polling rate (100Hz)
                }
            });
            _pollThread.IsBackground = true;
            _pollThread.Start();
        }

        public void StartEffect()
        {
            _isEffectActive = true;
            Console.WriteLine("[ReactiveEngine] Hook Active");
        }

        public void StopEffect()
        {
            _isEffectActive = false;
            Console.WriteLine("[ReactiveEngine] Hook Paused");
        }

        private int GetZoneForKey(int vkCode)
        {
            switch (vkCode)
            {
                case 0x1B: case 0xC0: case 0x09: case 0x14: // Esc, Tilde, Tab, Caps
                case 0xA0: case 0xA2: case 0x5B: case 0xA4: // LShift, LCtrl, LWin, LAlt
                case 0x10: case 0x11: case 0x12:            // Shift, Ctrl, Alt (generic)
                case 0x5A: case 0x58: case 0x43: case 0x41: case 0x53: case 0x44: // Z, X, C, A, S, D
                case 0x51: case 0x57: case 0x45: // Q, W, E
                case 0x31: case 0x32: case 0x33: // 1, 2, 3
                case 0x70: case 0x71: case 0x72: // F1, F2, F3
                    return 0;
                    
                case 0x34: case 0x35: case 0x36: // 4, 5, 6
                case 0x52: case 0x54: case 0x59: // R, T, Y
                case 0x46: case 0x47: case 0x48: // F, G, H
                case 0x56: case 0x42: case 0x20: // V, B, Space
                case 0x73: case 0x74: case 0x75: // F4, F5, F6
                    return 1;
                    
                case 0x37: case 0x38: case 0x39: case 0x30: // 7, 8, 9, 0
                case 0x55: case 0x49: case 0x4F: case 0x50: // U, I, O, P
                case 0x4A: case 0x4B: case 0x4C: // J, K, L
                case 0x4E: case 0x4D: case 0xBC: case 0xBE: // N, M, Comma, Period
                case 0xA5: // RAlt
                case 0x76: case 0x77: case 0x78: case 0x79: // F7, F8, F9, F10
                    return 2;
                    
                default:
                    return 3;
            }
        }

        public void Dispose()
        {
            _isRunning = false;
        }
    }
}
