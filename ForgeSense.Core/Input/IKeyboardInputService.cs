using System.Collections.Concurrent;

namespace ForgeSense.Core.Input
{
    public interface IKeyboardInputService
    {
        void PushKeyEvent(string keyId, bool isKeyDown, float timestamp);
        bool TryDequeueEvent(out ReactiveEvent reactiveEvent);
        void ClearQueue();
    }
}
