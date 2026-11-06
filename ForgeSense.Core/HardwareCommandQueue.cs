using System;
using System.Collections.Concurrent;
using OpenRGB.NET;

namespace ForgeSense.Core
{
    public class HardwareCommandQueue
    {
        private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        public void Enqueue(Action command)
        {
            _queue.Enqueue(command);
        }

        public void ProcessQueue()
        {
            while (_queue.TryDequeue(out var command))
            {
                try
                {
                    command.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HardwareCommandQueue] Error processing command: {ex.Message}");
                }
            }
        }
    }
}
