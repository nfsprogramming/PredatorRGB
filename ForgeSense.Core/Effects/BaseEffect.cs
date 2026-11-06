using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public abstract class BaseEffect : ILightingEffect
    {
        public abstract string Id { get; }
        public abstract string DisplayName { get; }

        protected int LedCount;
        
        protected bool IsRunning { get; private set; }

        public virtual void Initialize(int ledCount)
        {
            LedCount = ledCount;
        }

        public virtual void Start()
        {
            IsRunning = true;
            Console.WriteLine($"[RGB] Effect start: {Id}");
        }

        public virtual void Stop()
        {
            IsRunning = false;
            Console.WriteLine($"[RGB] Effect stop: {Id}");
        }

        protected RgbState State = new RgbState();
        
        public virtual void SetState(RgbState state)
        {
            if (state.ColorVersion >= State.ColorVersion)
            {
                // Console.WriteLine($"[RGB] Applying latest state version: {state.ColorVersion} for {Id}");
                State = state.Clone();
            }
            else
            {
                Console.WriteLine($"[RGB] Ignoring stale state update version: {state.ColorVersion} (Current is {State.ColorVersion})");
            }
        }

        public abstract void Update(float time, float deltaTime, out Color[] colors);

        public static Color Interpolate(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            byte r = (byte)(a.R + (b.R - a.R) * t);
            byte g = (byte)(a.G + (b.G - a.G) * t);
            byte blue = (byte)(a.B + (b.B - a.B) * t);
            return new Color(r, g, blue);
        }

        public virtual void Trigger(string eventType)
        {
            // Default no-op for effects that don't support triggers
        }

        public virtual void Dispose()
        {
            if (IsRunning)
            {
                Stop();
            }
            Console.WriteLine($"[RGB] Effect disposal: {Id}");
        }
    }
}
