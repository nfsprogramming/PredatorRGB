using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 61. Hologram
    public class HologramEffect : BaseEffect
    {
        public override string Id => "hologram";
        public override string DisplayName => "Hologram";

        private Color _cyan = new Color(0, 255, 255);
        private Color _blue = new Color(0, 100, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = (time * State.Speed * 0.5f) % LedCount;
            if (phase < 0) phase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - phase);
                if (dist > LedCount / 2f) dist = LedCount - dist; // wrap

                // Faint base glow
                float intensity = 0.1f;
                
                // Holographic scanline
                if (dist < State.Width)
                {
                    intensity += (1.0f - (dist / State.Width)) * 0.8f;
                }

                // Oscillation
                intensity += (float)Math.Sin(time * State.Speed * 5f) * 0.1f;
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);

                colors[i] = Interpolate(_blue, _cyan, intensity);
            }
        }
    }

    // 62. Cyber Pulse
    public class CyberPulseEffect : BaseEffect
    {
        public override string Id => "cyber_pulse";
        public override string DisplayName => "Cyber Pulse";

        private Color _base = new Color(10, 0, 30); // dark purple
        private Color _pulse = new Color(0, 255, 255); // cyan

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = (time * State.Speed * 2f) % LedCount;
            if (phase < 0) phase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = phase - i;
                if (dist < 0) dist += LedCount;

                float intensity = 1.0f - (dist / Math.Max(0.1f, State.Trail * LedCount));
                intensity = Math.Clamp(intensity, 0f, 1f);
                intensity = (float)Math.Pow(intensity, 2.0); // Smooth exponential decay

                colors[i] = Interpolate(_base, _pulse, intensity * State.Intensity);
            }
        }
    }

    // 63. Digital Rain
    public class DigitalRainEffect : BaseEffect
    {
        public override string Id => "digital_rain";
        public override string DisplayName => "Digital Rain";

        private Color _darkGreen = new Color(0, 30, 0);
        private Color _brightGreen = new Color(0, 255, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];

            for (int i = 0; i < LedCount; i++)
            {
                // Each zone has a slightly different phase and frequency to simulate separate streams
                float streamPhase = time * State.Speed * (1.0f + (i * 0.2f));
                float noise = (float)(Math.Sin(streamPhase * 3.0f + i) + Math.Cos(streamPhase * 1.5f - i)) / 2f;
                
                float val = (noise + 1f) / 2f; // 0 to 1
                val = (float)Math.Pow(val, 3.0); // sharp peaks
                
                colors[i] = Interpolate(_darkGreen, _brightGreen, Math.Clamp(val * State.Intensity, 0f, 1f));
            }
        }
    }

    // 64. Quantum Flow
    public class QuantumFlowEffect : BaseEffect
    {
        public override string Id => "quantum_flow";
        public override string DisplayName => "Quantum Flow";

        private Color _blue = new Color(0, 0, 255);
        private Color _magenta = new Color(255, 0, 255);
        private Color _cyan = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];

            for (int i = 0; i < LedCount; i++)
            {
                float w1 = (float)Math.Sin(time * State.Speed + i * 1.2f);
                float w2 = (float)Math.Cos(time * State.Speed * 1.7f - i * 0.8f);
                float w3 = (float)Math.Sin(time * State.Speed * 0.5f + i * 2.1f);
                
                float combined = (w1 + w2 + w3) / 3f; // roughly -1 to +1
                float normalized = (combined + 1f) / 2f; // 0 to 1

                Color c;
                if (normalized < 0.5f) c = Interpolate(_blue, _magenta, normalized * 2f);
                else c = Interpolate(_magenta, _cyan, (normalized - 0.5f) * 2f);

                colors[i] = new Color(
                    (byte)(c.R * State.Intensity),
                    (byte)(c.G * State.Intensity),
                    (byte)(c.B * State.Intensity)
                );
            }
        }
    }

    // 65. Singularity
    public class SingularityEffect : BaseEffect
    {
        public override string Id => "singularity";
        public override string DisplayName => "Singularity";

        private Color _purple = new Color(128, 0, 128);
        private Color _white = new Color(255, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float cycle = (float)Math.Sin(time * State.Speed) * 0.5f + 0.5f; // 0 to 1

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - 1.5f); // 0.5 for center (1,2), 1.5 for outer (0,3)
                
                // When cycle = 1, center is bright, outer is dim
                // When cycle = 0, center is dim, outer is bright
                float targetIntensity = dist < 1.0f ? cycle : (1.0f - cycle);
                targetIntensity = Math.Clamp(targetIntensity * State.Intensity, 0.1f, 1.0f); // keep it slightly lit

                colors[i] = Interpolate(_purple, _white, targetIntensity);
            }
        }
    }

    // 66. Black Hole
    public class BlackHoleEffect : BaseEffect
    {
        public override string Id => "black_hole";
        public override string DisplayName => "Black Hole";

        private Color _dark = new Color(5, 0, 15);
        private Color _energy = new Color(150, 0, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            // Collapse cycle: Energy flows outside in, spikes at center, goes dark
            float phase = (time * State.Speed) % 1.0f; 
            
            for (int i = 0; i < LedCount; i++)
            {
                float distFromCenter = Math.Abs(i - 1.5f) / 1.5f; // 0 at center, 1 at edge
                
                // Distance maps to phase to simulate movement inward
                float wave = 1.0f - Math.Abs(distFromCenter - (1.0f - phase)); 
                wave = (float)Math.Pow(Math.Clamp(wave, 0f, 1f), 3.0); // sharp peak

                // When phase approaches 1 (collapse), everything goes dark
                float collapseFade = 1.0f;
                if (phase > 0.8f) collapseFade = 1.0f - ((phase - 0.8f) / 0.2f);
                
                colors[i] = Interpolate(_dark, _energy, wave * collapseFade * State.Intensity);
            }
        }
    }

    // 67. Wormhole
    public class WormholeEffect : BaseEffect
    {
        public override string Id => "wormhole";
        public override string DisplayName => "Wormhole";

        private Color[] _palette = new[] { 
            new Color(128, 0, 128), // Purple
            new Color(0, 0, 255),   // Blue
            new Color(0, 255, 255), // Cyan
            new Color(255, 20, 147) // Pink
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 2.0f;

            for (int i = 0; i < LedCount; i++)
            {
                // Rotating phase offsets
                float phase = (time * speed + (i * 0.75f)) % _palette.Length;
                if (phase < 0) phase += _palette.Length;

                int idx1 = (int)Math.Floor(phase);
                int idx2 = (idx1 + 1) % _palette.Length;
                
                Color c = Interpolate(_palette[idx1], _palette[idx2], phase - idx1);

                // Brightness concentration in center
                float dist = Math.Abs(i - 1.5f);
                float intensity = 1.0f - (dist * 0.2f);

                colors[i] = new Color(
                    (byte)(c.R * intensity * State.Intensity),
                    (byte)(c.G * intensity * State.Intensity),
                    (byte)(c.B * intensity * State.Intensity)
                );
            }
        }
    }

    // 68. Time Warp
    public class TimeWarpEffect : BaseEffect
    {
        public override string Id => "time_warp";
        public override string DisplayName => "Time Warp";

        private Color _base = new Color(0, 50, 150);
        private Color _highlight = new Color(0, 255, 255);
        private float _internalPhase = 0f;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Warp speed math without breaking global clock
            // speedMult oscillating between 0.2 and 2.5
            float speedMult = (float)Math.Sin(time * 0.5f) * 1.15f + 1.35f; 
            
            // Accumulate our own phase using the variable speed
            _internalPhase += deltaTime * State.Speed * speedMult * 5.0f;
            
            float phase = _internalPhase % LedCount;
            if (phase < 0) phase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - phase);
                if (dist > LedCount / 2f) dist = LedCount - dist;

                float intensity = 1.0f - (dist / Math.Max(0.5f, State.Width * 2f));
                intensity = Math.Clamp(intensity, 0f, 1f);

                colors[i] = Interpolate(_base, _highlight, intensity * State.Intensity);
            }
        }
    }

    // 69. Holographic Spectrum
    public class HolographicSpectrumEffect : BaseEffect
    {
        public override string Id => "holographic_spectrum";
        public override string DisplayName => "Holographic Spectrum";

        private Color[] _palette = new[] { 
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128), // Purple
            new Color(255, 20, 147),// Pink
            new Color(255, 0, 255)  // Magenta
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 0.3f; // slow and elegant

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * 0.3f; // broad spectrum width
                float pos = (time * speed + offset) % _palette.Length;
                if (pos < 0) pos += _palette.Length;

                int idx1 = (int)Math.Floor(pos);
                int idx2 = (idx1 + 1) % _palette.Length;

                Color baseC = Interpolate(_palette[idx1], _palette[idx2], pos - idx1);
                
                // Holographic intensity modulation (slight scanning flicker)
                float glow = (float)Math.Sin(time * 5f + i) * 0.15f + 0.85f;
                
                colors[i] = new Color(
                    (byte)(baseC.R * glow * State.Intensity),
                    (byte)(baseC.G * glow * State.Intensity),
                    (byte)(baseC.B * glow * State.Intensity)
                );
            }
        }
    }

    // 70. Cyber Pulse Wave
    public class CyberPulseWaveEffect : BaseEffect
    {
        public override string Id => "cyber_pulse_wave";
        public override string DisplayName => "Cyber Pulse Wave";

        private Color[] _wavePalette = new[] { 
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128)  // Purple
        };
        
        private Color _pulseColor = new Color(255, 255, 255); // White

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float waveSpeed = State.Speed * 0.5f;
            float pulseSpeed = State.Speed * 3.0f; // pulse travels faster

            float pulsePhase = (time * pulseSpeed) % LedCount;
            if (pulsePhase < 0) pulsePhase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                // Base wave
                float wavePos = (time * waveSpeed + i * 0.5f) % _wavePalette.Length;
                if (wavePos < 0) wavePos += _wavePalette.Length;
                int idx1 = (int)Math.Floor(wavePos);
                int idx2 = (idx1 + 1) % _wavePalette.Length;
                Color baseC = Interpolate(_wavePalette[idx1], _wavePalette[idx2], wavePos - idx1);

                // Pulse overlay
                float dist = pulsePhase - i;
                if (dist < 0) dist += LedCount; // wrap tail
                
                float pulseIntensity = 1.0f - (dist / Math.Max(0.5f, State.Trail * LedCount));
                pulseIntensity = Math.Clamp(pulseIntensity, 0f, 1f);
                pulseIntensity = (float)Math.Pow(pulseIntensity, 2.0); // sharp decay

                colors[i] = Interpolate(baseC, _pulseColor, pulseIntensity * State.Intensity);
            }
        }
    }
}
