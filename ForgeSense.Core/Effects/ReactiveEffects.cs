using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;
using ForgeSense.Core.Input;

namespace ForgeSense.Core.Effects
{
    // 71. Key Reactive
    public class KeyReactiveEffect : BaseReactiveEffect
    {
        public override string Id => "key_reactive";
        public override string DisplayName => "Key Reactive";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            // Process new events
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneIntensities[e.PhysicalZone] = 1.0f; // Max brightness
                }
            }

            // Decay over time
            float decayRate = State.Speed * 2.0f;
            for (int i = 0; i < LedCount; i++)
            {
                ZoneIntensities[i] -= deltaTime * decayRate;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.G * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.B * ZoneIntensities[i] * State.Intensity)
                );
            }
        }
    }

    // 72. Zone Reactive
    public class ZoneReactiveEffect : BaseReactiveEffect
    {
        public override string Id => "zone_reactive";
        public override string DisplayName => "Zone Reactive";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneIntensities[e.PhysicalZone] = 1.0f;
                }
            }

            float decayRate = State.Speed * 0.5f; // Slower fade than Key Reactive
            for (int i = 0; i < LedCount; i++)
            {
                ZoneIntensities[i] -= deltaTime * decayRate;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.G * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.B * ZoneIntensities[i] * State.Intensity)
                );
            }
        }
    }

    // 73. Ripple Reactive
    public class RippleReactiveEffect : BaseReactiveEffect
    {
        public override string Id => "ripple_reactive";
        public override string DisplayName => "Ripple Reactive";

        // Store the timestamp of the last ripple and its origin
        private float _lastRippleTime = -10f;
        private int _rippleOrigin = 0;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    _lastRippleTime = time;
                    _rippleOrigin = e.PhysicalZone;
                }
            }

            float elapsed = time - _lastRippleTime;
            float speed = State.Speed * 4.0f; // Fast ripple

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - _rippleOrigin);
                
                // Expanding wave logic
                float waveRadius = elapsed * speed;
                float intensity = 0f;

                if (waveRadius >= dist && waveRadius < dist + State.Width)
                {
                    // Inside the ripple
                    intensity = 1.0f - ((waveRadius - dist) / State.Width);
                    intensity = Math.Clamp(intensity, 0f, 1f);
                }

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity * State.Intensity),
                    (byte)(State.CurrentColor.G * intensity * State.Intensity),
                    (byte)(State.CurrentColor.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 74. Wave Reactive
    public class WaveReactiveEffect : BaseReactiveEffect
    {
        public override string Id => "wave_reactive";
        public override string DisplayName => "Wave Reactive";

        private float _lastWaveTime = -10f;
        private int _waveOrigin = 0;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    _lastWaveTime = time;
                    _waveOrigin = e.PhysicalZone;
                }
            }

            float elapsed = time - _lastWaveTime;
            float speed = State.Speed * 3.0f;
            float wavePos = _waveOrigin + (elapsed * speed * State.Direction);

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - wavePos);
                float intensity = 1.0f - (dist / Math.Max(0.5f, State.Width));
                intensity = Math.Clamp(intensity, 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity * State.Intensity),
                    (byte)(State.CurrentColor.G * intensity * State.Intensity),
                    (byte)(State.CurrentColor.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 75. Key Trail
    public class KeyTrailEffect : BaseReactiveEffect
    {
        public override string Id => "key_trail";
        public override string DisplayName => "Key Trail";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    // Additive intensity (stackable trail)
                    ZoneIntensities[e.PhysicalZone] = Math.Min(1.0f, ZoneIntensities[e.PhysicalZone] + 0.5f);
                }
            }

            float decayRate = State.Speed * 0.2f; // Very slow fade
            for (int i = 0; i < LedCount; i++)
            {
                ZoneIntensities[i] -= deltaTime * decayRate;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.G * ZoneIntensities[i] * State.Intensity),
                    (byte)(State.CurrentColor.B * ZoneIntensities[i] * State.Intensity)
                );
            }
        }
    }

    // 76. Reactive Burst
    public class ReactiveBurstEffect : BaseReactiveEffect
    {
        public override string Id => "reactive_burst";
        public override string DisplayName => "Reactive Burst";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneIntensities[e.PhysicalZone] = 1.0f; // Fast attack
                }
            }

            float decayRate = State.Speed * 3.0f; // Fast decay
            for (int i = 0; i < LedCount; i++)
            {
                ZoneIntensities[i] -= deltaTime * decayRate;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);
                
                // Exponential decay curve for burst feel
                float burst = (float)Math.Pow(ZoneIntensities[i], 3.0);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * burst * State.Intensity),
                    (byte)(State.CurrentColor.G * burst * State.Intensity),
                    (byte)(State.CurrentColor.B * burst * State.Intensity)
                );
            }
        }
    }

    // 77. Reactive Spectrum
    public class ReactiveSpectrumEffect : BaseReactiveEffect
    {
        public override string Id => "reactive_spectrum";
        public override string DisplayName => "Reactive Spectrum";

        private Color[] _spectrum = new[] {
            new Color(255, 0, 0),
            new Color(255, 128, 0),
            new Color(255, 255, 0),
            new Color(0, 255, 0),
            new Color(0, 255, 255),
            new Color(0, 0, 255),
            new Color(128, 0, 128),
            new Color(255, 0, 255)
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneIntensities[e.PhysicalZone] = 1.0f;
                    ZoneKeyCounts[e.PhysicalZone]++; // Increment hue
                }
            }

            float decayRate = State.Speed * 0.5f;
            for (int i = 0; i < LedCount; i++)
            {
                ZoneIntensities[i] -= deltaTime * decayRate;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);

                int colorIdx = ZoneKeyCounts[i] % _spectrum.Length;
                Color targetC = _spectrum[colorIdx];

                colors[i] = new Color(
                    (byte)(targetC.R * ZoneIntensities[i] * State.Intensity),
                    (byte)(targetC.G * ZoneIntensities[i] * State.Intensity),
                    (byte)(targetC.B * ZoneIntensities[i] * State.Intensity)
                );
            }
        }
    }

    // 78. Reactive Pulse
    public class ReactivePulseEffect : BaseReactiveEffect
    {
        public override string Id => "reactive_pulse";
        public override string DisplayName => "Reactive Pulse";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneTimestamps[e.PhysicalZone] = time; // Start pulse
                }
            }

            for (int i = 0; i < LedCount; i++)
            {
                float intensity = 0f;
                if (ZoneTimestamps[i] > 0)
                {
                    float elapsed = time - ZoneTimestamps[i];
                    if (elapsed < 1.0f / State.Speed) // One full pulse
                    {
                        intensity = (float)Math.Sin(elapsed * State.Speed * Math.PI); // 0 -> 1 -> 0
                        intensity = Math.Clamp(intensity, 0f, 1f);
                    }
                    else
                    {
                        ZoneTimestamps[i] = -1f; // End pulse
                    }
                }

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity * State.Intensity),
                    (byte)(State.CurrentColor.G * intensity * State.Intensity),
                    (byte)(State.CurrentColor.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 79. Reactive Shockwave
    public class ReactiveShockwaveEffect : BaseReactiveEffect
    {
        public override string Id => "reactive_shockwave";
        public override string DisplayName => "Reactive Shockwave";

        private float _lastShockTime = -10f;
        private int _shockOrigin = 0;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    _lastShockTime = time;
                    _shockOrigin = e.PhysicalZone;
                }
            }

            float elapsed = time - _lastShockTime;
            
            for (int i = 0; i < LedCount; i++)
            {
                // Distance based falloff
                float dist = Math.Abs(i - _shockOrigin);
                float maxIntensity = 1.0f - (dist * 0.2f); // Further zones peak lower
                
                // Global fade
                float intensity = maxIntensity - (elapsed * State.Speed);
                intensity = Math.Clamp(intensity, 0f, 1f);
                intensity = (float)Math.Pow(intensity, 2.0);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity * State.Intensity),
                    (byte)(State.CurrentColor.G * intensity * State.Intensity),
                    (byte)(State.CurrentColor.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 80. Reactive Plasma
    public class ReactivePlasmaEffect : BaseReactiveEffect
    {
        public override string Id => "reactive_plasma";
        public override string DisplayName => "Reactive Plasma";

        private Color _base = new Color(0, 0, 50); // Dark Blue
        private Color _highlight = new Color(0, 255, 255); // Cyan

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            var events = ProcessEvents();
            
            foreach (var e in events)
            {
                if (e.IsKeyDown)
                {
                    ZoneIntensities[e.PhysicalZone] = 1.0f; // Disturbance
                }
            }

            for (int i = 0; i < LedCount; i++)
            {
                // Base plasma loop
                float plasmaPhase = time * State.Speed;
                float noise = (float)(Math.Sin(plasmaPhase + i) + Math.Cos(plasmaPhase * 0.7 - i)) / 2f;
                float baseIntensity = (noise + 1f) / 2f * 0.3f; // 0 to 0.3

                // Reactive decay
                ZoneIntensities[i] -= deltaTime * State.Speed * 1.5f;
                ZoneIntensities[i] = Math.Clamp(ZoneIntensities[i], 0f, 1f);

                // Composite
                float finalIntensity = baseIntensity + (ZoneIntensities[i] * 0.7f);

                colors[i] = Interpolate(_base, _highlight, finalIntensity * State.Intensity);
            }
        }
    }
}
