using System.Collections.Concurrent;

namespace ForgeSense.Core.Input
{
    public class KeyboardInputService : IKeyboardInputService
    {
        private static readonly KeyboardInputService _instance = new KeyboardInputService();
        public static KeyboardInputService Instance => _instance;

        private readonly ConcurrentQueue<ReactiveEvent> _eventQueue = new ConcurrentQueue<ReactiveEvent>();

        private KeyboardInputService() { }

        public void PushKeyEvent(string keyId, bool isKeyDown, float timestamp)
        {
            int zone = KeyboardZoneMapper.GetZoneForKey(keyId);
            _eventQueue.Enqueue(new ReactiveEvent(keyId, zone, isKeyDown, timestamp));
        }

        public bool TryDequeueEvent(out ReactiveEvent reactiveEvent)
        {
            return _eventQueue.TryDequeue(out reactiveEvent);
        }

        public void ClearQueue()
        {
            while (_eventQueue.TryDequeue(out _)) { }
        }
    }
}
