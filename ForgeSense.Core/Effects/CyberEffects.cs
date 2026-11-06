using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 41. Ice
    public class IceEffect : BaseEffect
    {
        public override string Id => "ice";
        public override string DisplayName => "Ice";

        private Color[] _palette = new[] { 
            new Color(0, 0, 139),     // Deep Blue
            new Color(173, 216, 230), // Ice Blue
            new Color(0, 255, 255),   // Cyan
            new Color(255, 255, 255)  // White
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 0.5f;

            for (int i = 0; i < LedCount; i++)
            {
                // Smooth deterministic shimmer
                float noise = (float)(Math.Sin(time * speed + i * 1.5) + Math.Cos(time * speed * 2.1 - i)) / 2f;
                float val = (noise + 1f) / 2f; // 0 to 1
                
                // Keep it mostly in the lower spectrum (Blue/Cyan), occasionally hitting White
                val = (float)Math.Pow(val, 2.0); // Bias towards 0
                val *= State.Intensity;

                float scaledPos = val * (_palette.Length - 1);
                int idx1 = (int)Math.Floor(scaledPos);
                int idx2 = Math.Min(idx1 + 1, _palette.Length - 1);

                colors[i] = Interpolate(_palette[idx1], _palette[idx2], scaledPos - idx1);
            }
        }
    }

    // 42. Thunder
    public class ThunderEffect : BaseEffect
    {
        public override string Id => "thunder";
        public override string DisplayName => "Thunder";

        private Color _darkBase = new Color(5, 0, 15);
        private Color _flash = new Color(200, 230, 255);
        
        // Random deterministic state based on time
        private float _lastFlashTime = -10f;
        private int _flashType = 0; // 0 = single, 1 = double flash

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float freq = 4.0f; // time between flashes

            // Check if we need to trigger a new flash
            if (time - _lastFlashTime > freq)
            {
                _lastFlashTime = time;
                _flashType = ((int)(time * 100)) % 2; // pseudo-random double flash
            }

            float timeSinceFlash = time - _lastFlashTime;
            float flashIntensity = 0f;

            // Attack/Peak/Decay
            if (timeSinceFlash < 0.1f)
            {
                flashIntensity = timeSinceFlash / 0.1f; // Attack
            }
            else if (timeSinceFlash < 0.6f)
            {
                flashIntensity = 1.0f - ((timeSinceFlash - 0.1f) / 0.5f); // Decay
                flashIntensity = (float)Math.Pow(flashIntensity, 2);

                // Double flash logic
                if (_flashType == 1 && timeSinceFlash > 0.15f && timeSinceFlash < 0.25f)
                {
                    flashIntensity += 0.8f;
                }
            }

            flashIntensity = Math.Clamp(flashIntensity * State.Intensity, 0f, 1f);

            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = Interpolate(_darkBase, _flash, flashIntensity);
            }
        }
    }

    // 43. Electric Storm
    public class ElectricStormEffect : BaseEffect
    {
        public override string Id => "electric_storm";
        public override string DisplayName => "Electric Storm";

        private Color _dark = new Color(0, 0, 0);
        private Color _electric = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            for (int i = 0; i < LedCount; i++)
            {
                // High frequency chaotic pulses
                float noise = (float)Math.Sin(time * State.Speed * 10f + i * 4f);
                noise += (float)Math.Cos(time * State.Speed * 15f - i * 2f);
                
                float val = (noise + 2f) / 4f; // roughly 0 to 1
                val = (float)Math.Pow(val, 4.0); // very sharp spikes

                colors[i] = Interpolate(_dark, _electric, val * State.Intensity);
            }
        }
    }

    // 44. Neon Flux
    public class NeonFluxEffect : BaseEffect
    {
        public override string Id => "neon_flux";
        public override string DisplayName => "Neon Flux";

        private Color[] _palette = new[] { 
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128), // Purple
            new Color(255, 0, 255)  // Magenta
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Speed;

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * 0.5f;
                float pos = (phase + offset) % _palette.Length;
                if (pos < 0) pos += _palette.Length;

                int idx1 = (int)Math.Floor(pos);
                int idx2 = (idx1 + 1) % _palette.Length;

                colors[i] = Interpolate(_palette[idx1], _palette[idx2], pos - idx1);
            }
        }
    }

    // 45. Synthwave
    public class SynthwaveEffect : BaseEffect
    {
        public override string Id => "synthwave";
        public override string DisplayName => "Synthwave";

        private Color _pink = new Color(255, 20, 147);
        private Color _purple = new Color(128, 0, 128);
        private Color _blue = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            float overarchingPulse = (float)Math.Sin(time * State.Speed) * 0.5f + 0.5f;

            for (int i = 0; i < LedCount; i++)
            {
                // Gradient base Pink/Purple
                Color baseColor = Interpolate(_pink, _purple, (float)i / (LedCount - 1));
                
                // Highlight pulse
                Color finalColor = Interpolate(baseColor, _blue, overarchingPulse * 0.5f);
                colors[i] = finalColor;
            }
        }
    }

    // 46. Matrix
    public class MatrixEffect : BaseEffect
    {
        public override string Id => "matrix";
        public override string DisplayName => "Matrix";

        private Color _darkGreen = new Color(0, 40, 0);
        private Color _brightGreen = new Color(0, 255, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                double flow = ((double)i / 4.0 - time * 2.0) % 1.0;
                if (flow < 0) flow += 1.0;
                double intensity = flow > 0.7 ? (flow - 0.7) / 0.3 : 0.0;
                byte g = (byte)Math.Clamp((int)(255 * intensity), 0, 255);
                colors[i] = new Color(0, g, 0);
            }
        }
    }

    // 47. Digital Glitch
    public class DigitalGlitchEffect : BaseEffect
    {
        public override string Id => "digital_glitch";
        public override string DisplayName => "Digital Glitch";

        private Color[] _palette = new[] { 
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128), // Purple
            new Color(255, 0, 255)  // Pink
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Generate deterministic glitch threshold based on time
            bool isGlitch = false;
            float glitchVal = (float)Math.Sin(time * 45f) * (float)Math.Cos(time * 23f);
            if (glitchVal > 0.85f)
            {
                isGlitch = true;
            }

            for (int i = 0; i < LedCount; i++)
            {
                int colorIdx = i % _palette.Length;
                Color baseC = _palette[colorIdx];

                if (isGlitch)
                {
                    // Swap R and B for glitch effect
                    colors[i] = new Color(baseC.B, baseC.G, baseC.R);
                }
                else
                {
                    colors[i] = baseC;
                }
            }
        }
    }

    // 48. Energy Core
    public class EnergyCoreEffect : BaseEffect
    {
        public override string Id => "energy_core";
        public override string DisplayName => "Energy Core";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float pulse = (float)Math.Sin(time * State.Speed * Math.PI) * 0.5f + 0.5f;

            for (int i = 0; i < LedCount; i++)
            {
                // Distance from center (1.5)
                float dist = Math.Abs(i - 1.5f);
                
                // Center (dist=0.5) is bright, edges (dist=1.5) are dim, pulsating
                float intensity = 1.0f - (dist * 0.4f); // baseline
                intensity += (pulse * 0.3f); // add pulse
                
                // When pulse is high, edges light up more
                if (dist > 1.0f)
                {
                    intensity *= pulse; 
                }

                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 49. Power Surge
    public class PowerSurgeEffect : BaseEffect
    {
        public override string Id => "power_surge";
        public override string DisplayName => "Power Surge";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Ping pong phase
            float phase = (float)(Math.Sin(time * State.Speed) + 1) / 2f; // 0 to 1
            float position = phase * (LedCount - 1);

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - position);
                float intensity = 1.0f - (dist / Math.Max(0.1f, State.Trail * LedCount));
                
                intensity = (float)Math.Pow(Math.Clamp(intensity, 0f, 1f), 1.5);
                
                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 50. Cyber Grid
    public class CyberGridEffect : BaseEffect
    {
        public override string Id => "cyber_grid";
        public override string DisplayName => "Cyber Grid";

        private Color[] _palette = new[] { 
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128)  // Purple
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Discrete shifting (not smooth interpolation)
            int step = (int)(time * State.Speed);

            for (int i = 0; i < LedCount; i++)
            {
                int colorIdx = (i + step) % _palette.Length;
                if (colorIdx < 0) colorIdx += _palette.Length;
                
                // Add a scanning pulse
                float pulsePhase = (time * State.Speed * 2f) % LedCount;
                if (pulsePhase < 0) pulsePhase += LedCount;
                float pulseDist = Math.Abs(i - pulsePhase);
                
                Color baseC = _palette[colorIdx];
                if (pulseDist < 0.5f)
                {
                    // Scanline brightens the grid
                    colors[i] = new Color(
                        (byte)Math.Min(255, baseC.R + 100),
                        (byte)Math.Min(255, baseC.G + 100),
                        (byte)Math.Min(255, baseC.B + 100)
                    );
                }
                else
                {
                    colors[i] = baseC;
                }
            }
        }
    }
}
