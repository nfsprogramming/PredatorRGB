using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 91. Firestorm
    public class FirestormEffect : BaseEffect
    {
        public override string Id => "firestorm";
        public override string DisplayName => "Firestorm";

        private Color _darkRed = new Color(50, 0, 0);
        private Color _orange = new Color(255, 69, 0);
        private Color _yellow = new Color(255, 255, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Speed * 2.0f;

            for (int i = 0; i < LedCount; i++)
            {
                // Layered noise for organic fire flow
                float wave1 = (float)Math.Sin(phase - i * 1.5f);
                float wave2 = (float)Math.Cos(phase * 1.7f - i * 0.8f);
                float combined = (wave1 + wave2) / 2f;
                
                float normalized = (combined + 1f) / 2f; // 0 to 1
                
                Color c;
                if (normalized < 0.5f) c = Interpolate(_darkRed, _orange, normalized * 2f);
                else c = Interpolate(_orange, _yellow, (normalized - 0.5f) * 2f);

                // Add slight chaotic flicker
                float flicker = (float)Math.Sin(time * 15f + i * 5f) * 0.1f + 0.9f;

                colors[i] = new Color(
                    (byte)(c.R * flicker * State.Intensity),
                    (byte)(c.G * flicker * State.Intensity),
                    (byte)(c.B * flicker * State.Intensity)
                );
            }
        }
    }

    // 92. Ocean Pulse
    public class OceanPulseEffect : BaseEffect
    {
        public override string Id => "ocean_pulse";
        public override string DisplayName => "Ocean Pulse";

        private Color _deepBlue = new Color(0, 0, 50);
        private Color _cyan = new Color(0, 200, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Speed * 0.5f;

            for (int i = 0; i < LedCount; i++)
            {
                // Center is deeper (brighter), outer is darker
                float dist = Math.Abs(i - 1.5f);
                float baseDepth = 1.0f - (dist * 0.3f); 
                
                float wave = (float)Math.Sin(phase + i * 0.5f) * 0.5f + 0.5f;
                float finalIntensity = baseDepth * wave;

                colors[i] = Interpolate(_deepBlue, _cyan, finalIntensity * State.Intensity);
            }
        }
    }

    // 93. Aurora Borealis
    public class AuroraBorealisEffect : BaseEffect
    {
        public override string Id => "aurora_borealis";
        public override string DisplayName => "Aurora Borealis";

        private Color _green = new Color(0, 255, 100);
        private Color _purple = new Color(150, 0, 255);
        private Color _cyan = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Speed * 0.3f; // Slow, majestic

            for (int i = 0; i < LedCount; i++)
            {
                float w1 = (float)Math.Sin(phase + i);
                float w2 = (float)Math.Sin(phase * 1.5f - i * 1.2f);
                float w3 = (float)Math.Cos(phase * 0.7f + i * 0.5f);

                float combined = (w1 + w2 + w3) / 3f;
                float normalized = (combined + 1f) / 2f; // 0 to 1

                Color c;
                if (normalized < 0.5f) c = Interpolate(_green, _cyan, normalized * 2f);
                else c = Interpolate(_cyan, _purple, (normalized - 0.5f) * 2f);

                float glow = (float)Math.Sin(time + i) * 0.2f + 0.8f;

                colors[i] = new Color(
                    (byte)(c.R * glow * State.Intensity),
                    (byte)(c.G * glow * State.Intensity),
                    (byte)(c.B * glow * State.Intensity)
                );
            }
        }
    }

    // 94. Cosmic Dust
    public class CosmicDustEffect : BaseEffect
    {
        public override string Id => "cosmic_dust";
        public override string DisplayName => "Cosmic Dust";

        private Color _space = new Color(5, 0, 15);
        private Color _dust = new Color(255, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            for (int i = 0; i < LedCount; i++)
            {
                // Pseudo-random slow particles
                float t = time * State.Speed * 0.5f;
                float noise = (float)(Math.Sin(t * 3.1f + i * 17.3f) * Math.Cos(t * 2.7f - i * 13.1f));
                
                // Keep only sharp peaks
                float dustIntensity = 0f;
                if (noise > 0.8f)
                {
                    dustIntensity = (noise - 0.8f) * 5f; // scale 0 to 1
                }

                colors[i] = Interpolate(_space, _dust, dustIntensity * State.Intensity);
            }
        }
    }

    // 95. Solar Flare
    public class SolarFlareEffect : BaseEffect
    {
        public override string Id => "solar_flare";
        public override string DisplayName => "Solar Flare";

        private Color _base = new Color(30, 10, 0); // Dark orange
        private Color _flare = new Color(255, 255, 255); // White hot

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Generate periodic steep flare
            float phase = time * State.Speed * 0.5f; // Slow build up
            float wave = (float)Math.Sin(phase);
            
            float flareIntensity = 0f;
            if (wave > 0.9f) // Sharp peak at top of sine wave
            {
                flareIntensity = (wave - 0.9f) * 10f; // Scale 0.9-1.0 to 0.0-1.0
                flareIntensity = (float)Math.Pow(flareIntensity, 3.0); // Extreme attack
            }

            for (int i = 0; i < LedCount; i++)
            {
                // Offset flare position slightly across zones
                float zoneWave = (float)Math.Sin(phase + (i * 0.2f));
                float zIntensity = 0f;
                if (zoneWave > 0.9f)
                {
                    zIntensity = (float)Math.Pow((zoneWave - 0.9f) * 10f, 3.0);
                }

                colors[i] = Interpolate(_base, _flare, zIntensity * State.Intensity);
            }
        }
    }

    // 96. Volcanic Core
    public class VolcanicCoreEffect : BaseEffect
    {
        public override string Id => "volcanic_core";
        public override string DisplayName => "Volcanic Core";

        private Color _outer = new Color(20, 0, 0); // Dark Crimson
        private Color _core = new Color(255, 100, 0); // Bright Orange/Yellow

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Speed * 1.5f;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - 1.5f); // 0.5 center, 1.5 edge
                float baseHeat = 1.0f - (dist * 0.4f); // 0.8 center, 0.4 edge
                
                float turbulence = (float)Math.Sin(phase + i * 2.0f) * 0.2f;
                float finalHeat = Math.Clamp(baseHeat + turbulence, 0f, 1f);

                colors[i] = Interpolate(_outer, _core, finalHeat * State.Intensity);
            }
        }
    }

    // 97. Ghost Mode
    public class GhostModeEffect : BaseEffect
    {
        public override string Id => "ghost_mode";
        public override string DisplayName => "Ghost Mode";

        private Color _ghost = new Color(0, 50, 100);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            for (int i = 0; i < LedCount; i++)
            {
                // Very slow, very subtle pulse
                float wave = (float)Math.Sin(time * State.Speed * 0.3f + i * 0.7f) * 0.5f + 0.5f;
                wave = Math.Clamp(wave, 0.05f, 0.4f); // Max 40% opacity

                colors[i] = new Color(
                    (byte)(_ghost.R * wave * State.Intensity),
                    (byte)(_ghost.G * wave * State.Intensity),
                    (byte)(_ghost.B * wave * State.Intensity)
                );
            }
        }
    }

    // 98. Cyber Samurai
    public class CyberSamuraiEffect : BaseEffect
    {
        public override string Id => "cyber_samurai";
        public override string DisplayName => "Cyber Samurai";

        private Color _bg = new Color(2, 0, 2);
        private Color _redSlash = new Color(255, 0, 50);
        private Color _cyanSlash = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Cycle: Red slash left-to-right, then Cyan slash right-to-left
            float cycle = (time * State.Speed * 1.5f) % 2.0f; 

            float redPhase = cycle < 1.0f ? (cycle * LedCount * 2f) - LedCount : -LedCount;
            float cyanPhase = cycle >= 1.0f ? ((cycle - 1.0f) * LedCount * 2f) - LedCount : -LedCount;
            cyanPhase = (LedCount - 1) - cyanPhase; // Reverse direction

            for (int i = 0; i < LedCount; i++)
            {
                float redDist = Math.Abs(i - redPhase);
                float cyanDist = Math.Abs(i - cyanPhase);

                float redIntensity = Math.Clamp(1.0f - (redDist / 1.5f), 0f, 1f);
                float cyanIntensity = Math.Clamp(1.0f - (cyanDist / 1.5f), 0f, 1f);

                // Sharp cut
                redIntensity = (float)Math.Pow(redIntensity, 2.0);
                cyanIntensity = (float)Math.Pow(cyanIntensity, 2.0);

                float totalIntensity = Math.Max(redIntensity, cyanIntensity);
                Color c = redIntensity > cyanIntensity ? _redSlash : _cyanSlash;
                
                if (totalIntensity < 0.05f)
                {
                    colors[i] = _bg;
                }
                else
                {
                    colors[i] = Interpolate(_bg, c, totalIntensity * State.Intensity);
                }
            }
        }
    }

    // 99. Predator Vision
    public class PredatorVisionEffect : BaseEffect
    {
        public override string Id => "predator_vision";
        public override string DisplayName => "Predator Vision";

        private Color _scan = new Color(255, 0, 0); // Deep Red
        private Color _trail = new Color(40, 0, 0); // Very dim trail
        
        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Smooth bouncing scanner (triangle wave)
            float period = 2.0f / State.Speed;
            float pos = (time % period) / period; // 0 to 1
            float scannerPos = pos < 0.5f ? pos * 2.0f : 2.0f - (pos * 2.0f); // 0 -> 1 -> 0
            
            scannerPos *= (LedCount - 1); // Scale to physical zones

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - scannerPos);
                float intensity = 1.0f - (dist / Math.Max(0.5f, State.Width * 1.5f));
                intensity = Math.Clamp(intensity, 0f, 1f);
                
                // Core is pure scan, edge is trail
                colors[i] = Interpolate(_trail, _scan, (float)Math.Pow(intensity, 1.5) * State.Intensity);
            }
        }
    }

    // 100. Ultimate Fusion
    public class UltimateFusionEffect : BaseEffect
    {
        public override string Id => "ultimate_fusion";
        public override string DisplayName => "Ultimate Fusion";

        private Color[] _palette = new[] {
            new Color(0, 255, 255), // Cyan
            new Color(0, 0, 255),   // Blue
            new Color(128, 0, 128), // Purple
            new Color(255, 0, 255), // Magenta
            new Color(255, 0, 0)    // Red
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed;

            // 1. Moving Energy Wave (position)
            float wavePos = (time * speed * 2.0f) % LedCount;
            
            // 2. Subtle Pulse (global oscillation)
            float pulse = (float)Math.Sin(time * speed * 0.8f) * 0.15f + 0.85f;

            // 3. Occasional Energy Burst
            float burstPhase = time * speed * 0.2f;
            float burstTrigger = (float)Math.Sin(burstPhase);
            float burstVal = burstTrigger > 0.95f ? (burstTrigger - 0.95f) * 20f : 0f;

            for (int i = 0; i < LedCount; i++)
            {
                // 4. Color Evolution (shifting hue per zone)
                float colorPhase = (time * speed * 0.3f + i * 0.4f) % _palette.Length;
                if (colorPhase < 0) colorPhase += _palette.Length;
                
                int idx1 = (int)Math.Floor(colorPhase);
                int idx2 = (idx1 + 1) % _palette.Length;
                Color baseC = Interpolate(_palette[idx1], _palette[idx2], colorPhase - idx1);

                // Wave intensity overlay
                float dist = Math.Abs(i - wavePos);
                if (dist > LedCount / 2f) dist = LedCount - dist;
                float waveIntensity = 1.0f - (dist / Math.Max(1.0f, State.Width));
                waveIntensity = Math.Clamp(waveIntensity, 0.3f, 1.0f); // keep 30% background gradient

                // Composite all layers
                float finalIntensity = waveIntensity * pulse + burstVal;
                finalIntensity = Math.Clamp(finalIntensity * State.Intensity, 0f, 1f);

                colors[i] = new Color(
                    (byte)(baseC.R * finalIntensity),
                    (byte)(baseC.G * finalIntensity),
                    (byte)(baseC.B * finalIntensity)
                );
            }
        }
    }
}
