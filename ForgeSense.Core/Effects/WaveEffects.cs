using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 21. Rainbow Wave
    public class RainbowWaveEffect : BaseEffect
    {
        public override string Id => "rainbow_wave";
        public override string DisplayName => "Rainbow Wave";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                double t = time * 2.0 + (i * 1.5 * State.Direction);
                byte r = (byte)Math.Clamp((int)(Math.Sin(t) * 127 + 128), 0, 255);
                byte g = (byte)Math.Clamp((int)(Math.Sin(t + 2.0) * 127 + 128), 0, 255);
                byte b = (byte)Math.Clamp((int)(Math.Sin(t + 4.0) * 127 + 128), 0, 255);
                colors[i] = new Color(r, g, b);
            }
        }
    }

    // 22. Reverse Rainbow
    public class ReverseRainbowEffect : BaseEffect
    {
        public override string Id => "reverse_rainbow";
        public override string DisplayName => "Reverse Rainbow";

        private RainbowWaveEffect _internalWave = new RainbowWaveEffect();

        public override void Initialize(int ledCount)
        {
            base.Initialize(ledCount);
            _internalWave.Initialize(ledCount);
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            var revState = State.Clone();
            revState.Direction = -State.Direction; // Force reverse
            _internalWave.SetState(revState);
            _internalWave.Update(time, deltaTime, out colors);
        }
    }

    // 23. Color Morph
    public class ColorMorphEffect : BaseEffect
    {
        public override string Id => "color_morph";
        public override string DisplayName => "Color Morph";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            // Morph between two palettes. We use Color1..4 as Palette A, and shifted as Palette B.
            Color[] paletteA = new[] { State.CurrentColor, State.Color2, State.Color3, State.Color4 };
            Color[] paletteB = new[] { State.Color2, State.Color3, State.Color4, State.CurrentColor };

            // Smooth ping-pong morph
            float morphPhase = (float)(Math.Sin(time * Math.PI) + 1) / 2.0f;

            for (int i = 0; i < LedCount; i++)
            {
                int index = i % paletteA.Length;
                colors[i] = Interpolate(paletteA[index], paletteB[index], morphPhase);
            }
        }
    }

    // 24. Dual Wave
    public class DualWaveEffect : BaseEffect
    {
        public override string Id => "dual_wave";
        public override string DisplayName => "Dual Wave";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phaseA = time * State.Direction;
            float phaseB = -time * State.Direction; // Opposite direction

            for (int i = 0; i < LedCount; i++)
            {
                float offset = i * (1.0f / Math.Max(0.1f, State.Width));
                
                float waveA = (float)Math.Sin((phaseA + offset) * Math.PI * 2) * 0.5f + 0.5f;
                float waveB = (float)Math.Sin((phaseB + offset) * Math.PI * 2) * 0.5f + 0.5f;

                Color colorA = new Color(
                    (byte)(State.CurrentColor.R * waveA * State.Intensity),
                    (byte)(State.CurrentColor.G * waveA * State.Intensity),
                    (byte)(State.CurrentColor.B * waveA * State.Intensity)
                );

                Color colorB = new Color(
                    (byte)(State.Color2.R * waveB * State.Intensity),
                    (byte)(State.Color2.G * waveB * State.Intensity),
                    (byte)(State.Color2.B * waveB * State.Intensity)
                );

                // Blend via additive mixing
                colors[i] = new Color(
                    (byte)Math.Min(255, colorA.R + colorB.R),
                    (byte)Math.Min(255, colorA.G + colorB.G),
                    (byte)Math.Min(255, colorA.B + colorB.B)
                );
            }
        }
    }

    // 25. Opposite Wave
    public class OppositeWaveEffect : BaseEffect
    {
        public override string Id => "opposite_wave";
        public override string DisplayName => "Opposite Wave";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time * State.Direction;

            for (int i = 0; i < LedCount; i++)
            {
                // Distance from center determines offset
                float centerDist = Math.Abs(i - (LedCount - 1) / 2.0f);
                float offset = centerDist * (1.0f / Math.Max(0.1f, State.Width));
                
                // Ping-pong wave
                float wave = (float)Math.Sin((phase - offset) * Math.PI * 2) * 0.5f + 0.5f;

                // Center blend
                float centerBlend = 1.0f - (centerDist / ((LedCount - 1) / 2.0f)); // 1 at center, 0 at edge
                Color targetColor = Interpolate(State.CurrentColor, State.Color2, centerBlend * wave);

                colors[i] = new Color(
                    (byte)(targetColor.R * wave * State.Intensity),
                    (byte)(targetColor.G * wave * State.Intensity),
                    (byte)(targetColor.B * wave * State.Intensity)
                );
            }
        }
    }

    // 26. Center Out
    public class CenterOutEffect : BaseEffect
    {
        public override string Id => "center_out";
        public override string DisplayName => "Center Out";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time;

            for (int i = 0; i < LedCount; i++)
            {
                float centerDist = Math.Abs(i - (LedCount - 1) / 2.0f);
                float offset = centerDist * (1.0f / Math.Max(0.1f, State.Width));
                
                // Outward means phase - offset
                float wave = (float)Math.Sin((phase - offset) * Math.PI * 2) * 0.5f + 0.5f;

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * wave * State.Intensity),
                    (byte)(State.CurrentColor.G * wave * State.Intensity),
                    (byte)(State.CurrentColor.B * wave * State.Intensity)
                );
            }
        }
    }

    // 27. Outside In
    public class OutsideInEffect : BaseEffect
    {
        public override string Id => "outside_in";
        public override string DisplayName => "Outside In";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = time;

            for (int i = 0; i < LedCount; i++)
            {
                float centerDist = Math.Abs(i - (LedCount - 1) / 2.0f);
                float offset = centerDist * (1.0f / Math.Max(0.1f, State.Width));
                
                // Inward means phase + offset
                float wave = (float)Math.Sin((phase + offset) * Math.PI * 2) * 0.5f + 0.5f;

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * wave * State.Intensity),
                    (byte)(State.CurrentColor.G * wave * State.Intensity),
                    (byte)(State.CurrentColor.B * wave * State.Intensity)
                );
            }
        }
    }

    // 28. Zone Chase
    public class ZoneChaseEffect : BaseEffect
    {
        public override string Id => "zone_chase";
        public override string DisplayName => "Zone Chase";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = (time * State.Direction) % LedCount;
            if (phase < 0) phase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                // Calculate distance behind the active phase (wrapping around)
                float dist = phase - i;
                if (dist < 0) dist += LedCount; // wrap
                
                // If moving backwards, inverse distance
                if (State.Direction < 0)
                {
                    dist = i - phase;
                    if (dist < 0) dist += LedCount;
                }

                // Decay based on Trail length
                float intensity = 1.0f - (dist / Math.Max(0.1f, State.Trail * LedCount));
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);
                
                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 29. Zone Scanner
    public class ZoneScannerEffect : BaseEffect
    {
        public override string Id => "zone_scanner";
        public override string DisplayName => "Zone Scanner";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            // Ping pong: 0 -> LedCount-1 -> 0
            float length = Math.Max(1, LedCount - 1);
            float phase = time % (length * 2);
            float position = phase < length ? phase : (length * 2) - phase;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - position);
                float intensity = 1.0f - (dist / Math.Max(0.1f, State.Width));
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);
                
                // Non-linear falloff for smoother beam
                intensity = (float)Math.Pow(intensity, 2.0);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }

    // 30. RGB Radar
    public class RgbRadarEffect : BaseEffect
    {
        public override string Id => "rgb_radar";
        public override string DisplayName => "RGB Radar";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            float phase = (time * State.Direction) % LedCount;
            if (phase < 0) phase += LedCount;

            for (int i = 0; i < LedCount; i++)
            {
                float dist = Math.Abs(i - phase);
                // Wrap around distance for circular radar look
                if (dist > LedCount / 2.0f) dist = LedCount - dist;

                // Radar falloff (Gaussian-like)
                float intensity = (float)Math.Exp(-Math.Pow(dist / Math.Max(0.1f, State.Trail), 2.0));
                intensity = Math.Clamp(intensity * State.Intensity, 0f, 1f);

                colors[i] = new Color(
                    (byte)(State.CurrentColor.R * intensity),
                    (byte)(State.CurrentColor.G * intensity),
                    (byte)(State.CurrentColor.B * intensity)
                );
            }
        }
    }
}
