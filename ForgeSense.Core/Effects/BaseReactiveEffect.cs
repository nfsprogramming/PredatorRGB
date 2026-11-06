using System;
using System.Collections.Generic;
using ForgeSense.Core.Input;

namespace ForgeSense.Core.Effects
{
    public abstract class BaseReactiveEffect : BaseEffect
    {
        // 4 elements, one for each physical zone
        protected float[] ZoneIntensities = new float[4];
        
        // Optional: keep track of specific state variables per zone if needed by subclasses
        protected float[] ZoneTimestamps = new float[4];
        protected int[] ZoneKeyCounts = new int[4];

        public override void Start()
        {
            base.Start();
            // Ensure no stale events affect this new effect instance
            KeyboardInputService.Instance.ClearQueue();
            
            for (int i = 0; i < 4; i++)
            {
                ZoneIntensities[i] = 0f;
                ZoneTimestamps[i] = -1f;
                ZoneKeyCounts[i] = 0;
            }
        }

        public override void Stop()
        {
            base.Stop();
            // Clear any pending events when shutting down
            KeyboardInputService.Instance.ClearQueue();
        }

        protected List<ReactiveEvent> ProcessEvents()
        {
            List<ReactiveEvent> events = new List<ReactiveEvent>();
            while (KeyboardInputService.Instance.TryDequeueEvent(out var evt))
            {
                events.Add(evt);
            }
            return events;
        }
    }
}
