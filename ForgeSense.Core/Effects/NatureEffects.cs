using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 31. Energy Ripple
    public class EnergyRippleEffect : BaseEffect
    {
        public override string Id => "energy_ripple";
        public override string DisplayName => "Energy Ripple";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                double dist = Math.Abs(i - 1.5);
                double t = time * 3.0 - dist * 1.5;
                byte r = (byte)Math.Clamp((int)(Math.Sin(t) * 127 + 128), 0, 255);
                byte g = (byte)Math.Clamp((int)(Math.Sin(t + 2.0) * 127 + 128), 0, 255);
                byte b = (byte)Math.Clamp((int)(Math.Sin(t + 4.0) * 127 + 128), 0, 255);
                colors[i] = new Color(r, g, b);
            }
        }
    }

    // 32. Fire
    public class FireEffect : BaseEffect
    {
        public override string Id => "fire";
        public override string DisplayName => "Fire";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                int seed = (int)(i * 10) + (int)(time * 10);
                var rand = new Random(seed);
                double randomOffset = rand.NextDouble();
                double hue = 10.0 + randomOffset * 30.0;
                double val = 0.6 + randomOffset * 0.4;
                colors[i] = ColorUtils.HsvToRgb(hue, 1.0, val);
            }
        }
    }

    // 33. Lava
    public class LavaEffect : BaseEffect
    {
        public override string Id => "lava";
        public override string DisplayName => "Lava";

        private Color _darkRed = new Color(80, 0, 0);
        private Color _red = new Color(200, 20, 0);
        private Color _orange = new Color(255, 100, 0);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float flow = State.Speed * State.Flow * 0.5f;

            for (int i = 0; i < LedCount; i++)
            {
                // Very slow, smooth waves
                float noise = (float)(Math.Sin(time * flow + i * State.Viscosity) 
                                      + Math.Cos(time * flow * 0.7 - i * State.Viscosity * 1.3)) / 2.0f;
                
                float val = (noise + 1f) / 2f;
                val = Math.Clamp(val * State.Intensity, 0f, 1f);

                Color c;
                if (val < 0.5f) c = Interpolate(_darkRed, _red, val / 0.5f);
                else c = Interpolate(_red, _orange, (val - 0.5f) / 0.5f);

                colors[i] = c;
            }
        }
    }

    // 34. Water
    public class WaterEffect : BaseEffect
    {
        public override string Id => "water";
        public override string DisplayName => "Water";

        private Color _deepBlue = new Color(0, 0, 139);
        private Color _cyan = new Color(0, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                float wave = (float)Math.Sin(time * State.Speed + i * State.Width) * 0.5f + 0.5f;
                colors[i] = Interpolate(_deepBlue, _cyan, wave);
            }
        }
    }

    // 35. Ocean
    public class OceanEffect : BaseEffect
    {
        public override string Id => "ocean";
        public override string DisplayName => "Ocean";

        private Color _navy = new Color(0, 0, 80);
        private Color _blue = new Color(0, 100, 255);
        private Color _teal = new Color(0, 128, 128);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                float deep = (float)Math.Sin(time * State.Speed * 0.3) * 0.5f + 0.5f;
                float mid = (float)Math.Sin(time * State.Speed * 0.7 + i) * 0.5f + 0.5f;
                float surface = (float)Math.Sin(time * State.Speed * 1.2 - i * 0.5) * 0.5f + 0.5f;

                float mix = (deep + mid + surface) / 3.0f;
                
                Color c;
                if (mix < 0.5f) c = Interpolate(_navy, _blue, mix / 0.5f);
                else c = Interpolate(_blue, _teal, (mix - 0.5f) / 0.5f);

                colors[i] = c;
            }
        }
    }

    // 36. Lightning Storm
    public class LightningStormEffect : BaseEffect
    {
        public override string Id => "lightning_storm";
        public override string DisplayName => "Lightning Storm";

        private Color _darkBlue = new Color(0, 0, 30);
        private Color _flash = new Color(200, 255, 255);
        private float _lastFlashTime = -10f;
        private int _flashZone = 0;

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            // Deterministic trigger based on time modulo
            // Use randomness param to affect frequency
            float freq = 3.0f - (State.Randomness * 2.0f); 
            if (time - _lastFlashTime > freq)
            {
                // Trigger flash
                _lastFlashTime = time;
                _flashZone = (int)(time * 100) % LedCount; // pseudo-random zone
            }

            float timeSinceFlash = time - _lastFlashTime;
            float flashIntensity = 0f;

            if (timeSinceFlash < 0.5f)
            {
                // Quick decay
                flashIntensity = 1.0f - (timeSinceFlash / 0.5f);
                flashIntensity = (float)Math.Pow(flashIntensity, 3); // sharp dropoff
                flashIntensity *= State.Intensity;
            }

            for (int i = 0; i < LedCount; i++)
            {
                // Flash propagates across zones
                float dist = Math.Abs(i - _flashZone);
                float localFlash = Math.Max(0, flashIntensity - (dist * 0.2f));

                colors[i] = Interpolate(_darkBlue, _flash, localFlash);
            }
        }
    }

    // 37. Aurora Sky
    public class AuroraSkyEffect : BaseEffect
    {
        public override string Id => "aurora_sky";
        public override string DisplayName => "Aurora Sky";

        private Color[] _palette = new[] { new Color(0,255,100), new Color(0,255,255), new Color(0,100,255), new Color(255,0,255) };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 0.2f; // very slow

            for (int i = 0; i < LedCount; i++)
            {
                float noise = (float)(Math.Sin(time * speed + i * 0.5f) + Math.Cos(time * speed * 1.3f - i * 0.3f)) / 2f;
                float val = (noise + 1f) / 2f;
                val = Math.Clamp(val, 0f, 1f);

                float scaledPos = val * (_palette.Length - 1);
                int idx1 = (int)Math.Floor(scaledPos);
                int idx2 = Math.Min(idx1 + 1, _palette.Length - 1);
                
                colors[i] = Interpolate(_palette[idx1], _palette[idx2], scaledPos - idx1);
            }
        }
    }

    // 38. Sunset
    public class SunsetEffect : BaseEffect
    {
        public override string Id => "sunset";
        public override string DisplayName => "Sunset";

        private Color[] _palette = new[] { new Color(75,0,130), new Color(255,0,255), new Color(255,0,0), new Color(255,140,0), new Color(255,215,0) };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float speed = State.Speed * 0.1f; // extremely slow
            
            float phase = (float)(Math.Sin(time * speed) + 1) / 2f; // ping-pong 0 to 1

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * 0.1f; // slight gradient across keyboard
                float val = Math.Clamp(phase + offset, 0f, 1f);

                float scaledPos = val * (_palette.Length - 1);
                int idx1 = (int)Math.Floor(scaledPos);
                int idx2 = Math.Min(idx1 + 1, _palette.Length - 1);

                colors[i] = Interpolate(_palette[idx1], _palette[idx2], scaledPos - idx1);
            }
        }
    }

    // 39. Meteor Shower
    public class MeteorShowerEffect : BaseEffect
    {
        public override string Id => "meteor_shower";
        public override string DisplayName => "Meteor Shower";

        private Color _darkBase = new Color(0, 0, 40);
        private Color _meteor = new Color(200, 255, 255);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                double streak = ((double)i / 4.0 - time * 2.0) % 1.0;
                if (streak < 0) streak += 1.0;
                if (streak > 0.8)
                {
                    colors[i] = new Color(0, 241, 254);
                }
                else if (streak > 0.4)
                {
                    double frac = Math.Clamp((streak - 0.4) / 0.4, 0.0, 1.0);
                    colors[i] = new Color(0, (byte)(120 * frac), (byte)(212 * frac));
                }
                else
                {
                    colors[i] = new Color(17, 17, 17);
                }
            }
        }
    }

    // 40. Galaxy
    public class GalaxyEffect : BaseEffect
    {
        public override string Id => "galaxy";
        public override string DisplayName => "Galaxy";

        private Color _darkSpace = new Color(10, 0, 30);
        private Color _nebulaCyan = new Color(0, 150, 255);
        private Color _nebulaMagenta = new Color(200, 0, 200);

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            
            for (int i = 0; i < LedCount; i++)
            {
                float noiseCyan = (float)(Math.Sin(time * State.Speed * 0.5f + i) + 1) / 2f;
                float noiseMagenta = (float)(Math.Cos(time * State.Speed * 0.4f - i * 1.5f) + 1) / 2f;

                Color c1 = Interpolate(_darkSpace, _nebulaCyan, noiseCyan * State.Density);
                Color c2 = Interpolate(_darkSpace, _nebulaMagenta, noiseMagenta * State.Density);

                // Additive blend
                colors[i] = new Color(
                    (byte)Math.Min(255, c1.R + c2.R),
                    (byte)Math.Min(255, c1.G + c2.G),
                    (byte)Math.Min(255, c1.B + c2.B)
                );
            }
        }
    }
}
