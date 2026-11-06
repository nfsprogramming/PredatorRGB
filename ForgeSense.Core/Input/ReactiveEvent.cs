using System;

namespace ForgeSense.Core.Input
{
    public struct ReactiveEvent
    {
        public string KeyId;
        public int PhysicalZone; // 0, 1, 2, or 3
        public bool IsKeyDown;
        public float Timestamp;

        public ReactiveEvent(string keyId, int physicalZone, bool isKeyDown, float timestamp)
        {
            KeyId = keyId;
            PhysicalZone = physicalZone;
            IsKeyDown = isKeyDown;
            Timestamp = timestamp;
        }
    }
}
