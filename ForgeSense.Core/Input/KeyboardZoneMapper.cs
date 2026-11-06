using System.Collections.Generic;

namespace ForgeSense.Core.Input
{
    public class KeyboardZoneMapper
    {
        // Simple mapping based on standard QWERTY layout. 
        // 0: Left (Esc, Tab, Caps, Shift, Z, A, Q, 1, 2, 3)
        // 1: Center-Left (4, 5, E, R, D, F, X, C, Space)
        // 2: Center-Right (6, 7, T, Y, G, H, V, B)
        // 3: Right (8+, U+, J+, N+, Enter, Backspace, Numpad)

        private static readonly Dictionary<string, int> ZoneMap = new Dictionary<string, int>
        {
            {"Escape", 0}, {"Tab", 0}, {"CapsLock", 0}, {"LShiftKey", 0}, {"LControlKey", 0}, {"LWin", 0}, {"LMenu", 0},
            {"D1", 0}, {"D2", 0}, {"D3", 0}, {"Q", 0}, {"W", 0}, {"A", 0}, {"S", 0}, {"Z", 0}, {"X", 0},

            {"D4", 1}, {"D5", 1}, {"E", 1}, {"R", 1}, {"D", 1}, {"F", 1}, {"C", 1}, {"V", 1}, {"Space", 1},

            {"D6", 2}, {"D7", 2}, {"T", 2}, {"Y", 2}, {"G", 2}, {"H", 2}, {"B", 2}, {"N", 2},

            {"D8", 3}, {"D9", 3}, {"D0", 3}, {"U", 3}, {"I", 3}, {"O", 3}, {"P", 3}, {"J", 3}, {"K", 3}, {"L", 3}, 
            {"M", 3}, {"Enter", 3}, {"Back", 3}, {"RShiftKey", 3}, {"RControlKey", 3}, {"RWin", 3}, {"RMenu", 3}
        };

        public static int GetZoneForKey(string keyId)
        {
            if (ZoneMap.TryGetValue(keyId, out int zone))
            {
                return zone;
            }
            
            // Default to right zone for numpad/arrows/unknown
            return 3; 
        }
    }
}
