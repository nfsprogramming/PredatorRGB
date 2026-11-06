using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public class ReactiveVariantEffect : BaseEffect
    {
        public override string Id => "reactive_zone_variant";
        public override string DisplayName => "Reactive Zone Variant";

        private readonly ReactiveEngine _reactive;

        public ReactiveVariantEffect(ReactiveEngine reactive)
        {
            _reactive = reactive;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                // Single-zone exclusivity
                if (i != _reactive.ActiveZone && _reactive.ActiveZone != -1)
                {
                    colors[i] = new Color(0, 0, 0); // Other zones OFF
                    continue;
                }

                float reactiveIntensity = 0f;

                if (_reactive.IsZoneHeld[i])
                {
                    // Held state
                    reactiveIntensity = 1.0f;
                }
                else if (_reactive.LastReleaseTimes[i] > 0)
                {
                    // Fade out after release
                    long timeSinceRelease = Environment.TickCount64 - _reactive.LastReleaseTimes[i];
                    if (timeSinceRelease < 500)
                    {
                        reactiveIntensity = 1.0f - (timeSinceRelease / 500f);
                    }
                }
                
                byte r = (byte)(State.CurrentColor.R * reactiveIntensity);
                byte g = (byte)(State.CurrentColor.G * reactiveIntensity);
                byte b = (byte)(State.CurrentColor.B * reactiveIntensity);
                colors[i] = new Color(r, g, b);
            }
        }
    }
}
