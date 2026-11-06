using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public static class ColorUtils
    {
        public static Color HsvToRgb(double h, double s, double v)
        {
            int hi = Convert.ToInt32(Math.Floor(h / 60)) % 6;
            double f = h / 60 - Math.Floor(h / 60);

            v = v * 255;
            int vInt = Convert.ToInt32(v);
            int p = Convert.ToInt32(v * (1 - s));
            int q = Convert.ToInt32(v * (1 - f * s));
            int t = Convert.ToInt32(v * (1 - (1 - f) * s));

            if (hi == 0)
                return new Color((byte)vInt, (byte)t, (byte)p);
            else if (hi == 1)
                return new Color((byte)q, (byte)vInt, (byte)p);
            else if (hi == 2)
                return new Color((byte)p, (byte)vInt, (byte)t);
            else if (hi == 3)
                return new Color((byte)p, (byte)q, (byte)vInt);
            else if (hi == 4)
                return new Color((byte)t, (byte)p, (byte)vInt);
            else
                return new Color((byte)vInt, (byte)p, (byte)q);
        }
    }

    // 11. Spectrum Flow
    public class SpectrumFlowEffect : BaseEffect
    {
        public override string Id => "spectrum_flow";
        public override string DisplayName => "Spectrum Flow";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                double speed = 0.5 + i * 0.3;
                double offset = i * 4.2;
                double twinkle = Math.Sin(time * speed + offset);
                if (twinkle > 0.6)
                {
                    double val = (twinkle - 0.6) / 0.4;
                    byte r = (byte)Math.Clamp((int)(10 + 245 * val), 0, 255);
                    byte g = (byte)Math.Clamp((int)(10 + 245 * val), 0, 255);
                    byte b = (byte)Math.Clamp((int)(50 + 205 * val), 0, 255);
                    colors[i] = new Color(r, g, b);
                }
                else
                {
                    colors[i] = new Color(10, 10, 50);
                }
            }
        }
    }

    // 12. Color Wave
    public class ColorWaveEffect : BaseEffect
    {
        public override string Id => "color_wave";
        public override string DisplayName => "Color Wave";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Direction;
            
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };

            for (int i = 0; i < LedCount; i++)
            {
                // Width controls how stretched the wave is
                float offset = (i / Math.Max(0.1f, State.Width)) * (1.0f / LedCount);
                float pos = (phase + offset) % 1.0f;
                if (pos < 0) pos += 1.0f;

                float scaledPos = pos * palette.Length;
                int index1 = (int)Math.Floor(scaledPos) % palette.Length;
                int index2 = (index1 + 1) % palette.Length;
                float t = scaledPos - (float)Math.Floor(scaledPos);
                
                // Sine easing for smooth wave
                float ease = (float)(1 - Math.Cos(t * Math.PI)) / 2f;
                colors[i] = Interpolate(palette[index1], palette[index2], ease);
            }
        }
    }

    // 13. Plasma
    public class PlasmaEffect : BaseEffect
    {
        public override string Id => "plasma";
        public override string DisplayName => "Plasma";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            
            for (int i = 0; i < LedCount; i++)
            {
                float x = i * State.Scale;
                float v = (float)Math.Sin(x + time);
                v += (float)Math.Sin((x + time) / 2.0f);
                v += (float)Math.Sin((x + time) / 3.0f);
                
                // Normalize to 0-1
                float normalized = (v + 3f) / 6f;
                normalized *= State.Intensity;
                normalized = Math.Clamp(normalized, 0f, 1f);
                
                float scaledPos = normalized * (palette.Length - 1);
                int index1 = (int)Math.Floor(scaledPos);
                int index2 = Math.Min(index1 + 1, palette.Length - 1);
                float t = scaledPos - index1;
                
                colors[i] = Interpolate(palette[index1], palette[index2], t);
            }
        }
    }

    // 14. Aurora
    public class AuroraEffect : BaseEffect
    {
        public override string Id => "aurora";
        public override string DisplayName => "Aurora";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            byte r = (byte)Math.Clamp((int)(Math.Sin(time) * 127 + 128), 0, 255);
            byte g = (byte)Math.Clamp((int)(Math.Sin(time + 2.0) * 127 + 128), 0, 255);
            byte b = (byte)Math.Clamp((int)(Math.Sin(time + 4.0) * 127 + 128), 0, 255);
            var c = new Color(r, g, b);
            for (int i = 0; i < LedCount; i++) colors[i] = c;
        }
    }

    // 15. Nebula
    public class NebulaEffect : BaseEffect
    {
        public override string Id => "nebula";
        public override string DisplayName => "Nebula";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            
            for (int i = 0; i < LedCount; i++)
            {
                float spatial = i * State.Density;
                float slowTime = time * 0.3f;
                
                float v1 = (float)Math.Sin(spatial + slowTime);
                float v2 = (float)Math.Cos(spatial * 1.5f - slowTime * 0.8f);
                
                float val = (v1 + v2 + 2f) / 4f; // 0 to 1
                val = Math.Clamp(val * State.Intensity, 0f, 1f);
                
                float scaledPos = val * (palette.Length - 1);
                int index1 = (int)Math.Floor(scaledPos);
                int index2 = Math.Min(index1 + 1, palette.Length - 1);
                float t = scaledPos - index1;
                
                colors[i] = Interpolate(palette[index1], palette[index2], t);
            }
        }
    }

    // 16. Liquid
    public class LiquidEffect : BaseEffect
    {
        public override string Id => "liquid";
        public override string DisplayName => "Liquid";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            
            for (int i = 0; i < LedCount; i++)
            {
                // High viscosity = slow delta scaling
                float fluidPhase = time * State.Flow * State.Direction;
                float offset = i * (1.0f / Math.Max(0.1f, State.Viscosity));
                
                float wave = (float)Math.Sin(fluidPhase + offset) * 0.5f + 0.5f;
                wave = (float)Math.Pow(wave, 1.5); // non-linear liquid feel
                
                float scaledPos = wave * (palette.Length - 1);
                int index1 = (int)Math.Floor(scaledPos);
                int index2 = Math.Min(index1 + 1, palette.Length - 1);
                float t = scaledPos - index1;
                
                colors[i] = Interpolate(palette[index1], palette[index2], t);
            }
        }
    }

    // 17. Energy Flow
    public class EnergyFlowEffect : BaseEffect
    {
        public override string Id => "energy_flow";
        public override string DisplayName => "Energy Flow";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Direction;
            float center = (phase % 1.0f);
            if (center < 0) center += 1.0f;
            center *= LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - center);
                // Wrap around distance
                if (dist > LedCount / 2f) dist = LedCount - dist;
                
                float intensity = 1.0f - (dist / Math.Max(0.1f, State.Width));
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);
                
                // Trail decay (asymmetrical)
                float directedDist = (i - center);
                if (State.Direction > 0 && directedDist > 0) intensity *= 0.2f; // sharper front
                if (State.Direction < 0 && directedDist < 0) intensity *= 0.2f;

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 18. Prism
    public class PrismEffect : BaseEffect
    {
        public override string Id => "prism";
        public override string DisplayName => "Prism";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Direction;

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * (1.0f / Math.Max(0.1f, State.Width));
                float pos = (phase + offset) % 1.0f;
                if (pos < 0) pos += 1.0f;
                
                // Multi-band spectrum (Red, Orange, Yellow, Green, Cyan, Blue, Purple, Pink)
                double h = pos * 360.0;
                colors[i] = ColorUtils.HsvToRgb(h, State.Saturation, 1.0);
            }
        }
    }

    // 19. Vortex
    public class VortexEffect : BaseEffect
    {
        public override string Id => "vortex";
        public override string DisplayName => "Vortex";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            
            float phase = time * State.Rotation;
            
            for (int i = 0; i < LedCount; i++)
            {
                // Simulate rotation by sweeping intensity across the 4 zones
                float angle = i * (float)(Math.PI * 2 / LedCount);
                float intensity = (float)Math.Sin(phase + angle) * 0.5f + 0.5f;
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);
                
                float scaledPos = intensity * (palette.Length - 1);
                int index1 = (int)Math.Floor(scaledPos);
                int index2 = Math.Min(index1 + 1, palette.Length - 1);
                float t = scaledPos - index1;
                
                colors[i] = Interpolate(palette[index1], palette[index2], t);
            }
        }
    }

    // 20. Color Drift
    public class ColorDriftEffect : BaseEffect
    {
        public override string Id => "color_drift";
        public override string DisplayName => "Color Drift";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Color[] palette = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            
            // Irregular but deterministic timing
            float t1 = time * 0.5f;
            float t2 = time * 0.7f;
            float drift = (float)(Math.Sin(t1) + Math.Sin(t2)) / 2f; // -1 to 1
            drift = (drift + 1f) / 2f; // 0 to 1
            
            // Apply smoothness factor
            drift = (float)Math.Pow(drift, State.Smoothness);
            
            float scaledPos = drift * (palette.Length - 1);
            int index1 = (int)Math.Floor(scaledPos);
            int index2 = Math.Min(index1 + 1, palette.Length - 1);
            float t = scaledPos - index1;
            
            Color c = Interpolate(palette[index1], palette[index2], t);
            
            for (int i = 0; i < LedCount; i++) colors[i] = c;
        }
    }
}
