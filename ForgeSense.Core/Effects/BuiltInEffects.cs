using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    // 1. Static
    public class StaticEffect : BaseEffect
    {
        public override string Id => "static";
        public override string DisplayName => "Static";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = State.CurrentColor;
            }
        }
    }

    // 2. Dual Color
    public class DualColorEffect : BaseEffect
    {
        public override string Id => "dual_color";
        public override string DisplayName => "Dual Color";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            if (LedCount >= 4)
            {
                colors[0] = State.CurrentColor;
                colors[1] = State.CurrentColor;
                colors[2] = State.Color2;
                colors[3] = State.Color2;
            }
            else
            {
                for (int i = 0; i < LedCount; i++)
                {
                    colors[i] = i < (LedCount / 2) ? State.CurrentColor : State.Color2;
                }
            }
        }
    }

    // 3. Triple Color
    public class TripleColorEffect : BaseEffect
    {
        public override string Id => "triple_color";
        public override string DisplayName => "Triple Color";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            if (LedCount >= 4)
            {
                colors[0] = State.CurrentColor;
                colors[1] = State.Color2;
                colors[2] = State.Color3;
                colors[3] = State.Color2; // As requested
            }
            else
            {
                for (int i = 0; i < LedCount; i++) colors[i] = State.CurrentColor;
            }
        }
    }

    // 4. Quad Color
    public class QuadColorEffect : BaseEffect
    {
        public override string Id => "quad_color";
        public override string DisplayName => "Quad Color";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            if (LedCount >= 4)
            {
                colors[0] = State.CurrentColor;
                colors[1] = State.Color2;
                colors[2] = State.Color3;
                colors[3] = State.Color4;
            }
            else
            {
                for (int i = 0; i < LedCount; i++) colors[i] = State.CurrentColor;
            }
        }
    }

    // 5. Gradient
    public class GradientEffect : BaseEffect
    {
        public override string Id => "gradient";
        public override string DisplayName => "Gradient";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                float t = LedCount > 1 ? (float)i / (LedCount - 1) : 0f;
                colors[i] = Interpolate(State.CurrentColor, State.Color2, t);
            }
        }
    }

    // 6. Color Cycle
    public class ColorCycleEffect : BaseEffect
    {
        public override string Id => "color_cycle";
        public override string DisplayName => "Color Cycle";

        private Color[] _palette = new Color[]
        {
            new Color(0, 0, 255),    // Blue
            new Color(128, 0, 128),  // Purple
            new Color(255, 105, 180),// Pink
            new Color(255, 0, 0),    // Red
            new Color(255, 165, 0),  // Orange
            new Color(255, 255, 0),  // Yellow
            new Color(0, 255, 0),    // Green
            new Color(0, 255, 255)   // Cyan
        };

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            float phase = time % _palette.Length;
            int currentIndex = (int)Math.Floor(phase);
            int nextIndex = (currentIndex + 1) % _palette.Length;
            float t = phase - currentIndex;

            Color c = Interpolate(_palette[currentIndex], _palette[nextIndex], t);

            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = c;
        }
    }

    // 7. Random Palette
    public class RandomPaletteEffect : BaseEffect
    {
        public override string Id => "random_palette";
        public override string DisplayName => "Random Palette";

        private Color[] _palette;
        private Random _random = new Random();
        private int _lastVersion = -1;

        private void GeneratePalette()
        {
            int size = Math.Max(2, State.PaletteSize);
            _palette = new Color[size];
            for (int i = 0; i < size; i++)
            {
                byte r = (byte)_random.Next(50, 256);
                byte g = (byte)_random.Next(50, 256);
                byte b = (byte)_random.Next(50, 256);
                _palette[i] = new Color(r, g, b);
            }
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            if (_palette == null || State.ColorVersion != _lastVersion)
            {
                GeneratePalette();
                _lastVersion = State.ColorVersion;
            }

            float phase = time % _palette.Length;
            int currentIndex = (int)Math.Floor(phase);
            int nextIndex = (currentIndex + 1) % _palette.Length;
            float t = phase - currentIndex;

            Color c = Interpolate(_palette[currentIndex], _palette[nextIndex], t);

            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = c;
        }
    }

    // 8. Breathing
    public class BreathingEffect : BaseEffect
    {
        public override string Id => "breathing";
        public override string DisplayName => "Breathing";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            float opacity = (float)((Math.Sin(time * 2.0) + 1.0) / 2.0 * 0.8 + 0.2);
            var baseCol = State.CurrentColor.R == 0 && State.CurrentColor.G == 0 && State.CurrentColor.B == 0
                ? new Color(0, 120, 212)
                : State.CurrentColor;

            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)Math.Clamp((int)(baseCol.R * opacity), 0, 255),
                    (byte)Math.Clamp((int)(baseCol.G * opacity), 0, 255),
                    (byte)Math.Clamp((int)(baseCol.B * opacity), 0, 255)
                );
            }
        }
    }

    // 9. Smooth Fade
    public class SmoothFadeEffect : BaseEffect
    {
        public override string Id => "smooth_fade";
        public override string DisplayName => "Smooth Fade";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            // Ping-pong between Color A and Color B
            float phase = (float)(Math.Sin(time * Math.PI) + 1) / 2.0f;
            Color c = Interpolate(State.CurrentColor, State.Color2, phase);

            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = c;
        }
    }

    // 10. Pulse (Heartbeat)
    public class PulseEffect : BaseEffect
    {
        public override string Id => "pulse";
        public override string DisplayName => "Pulse";

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            float beat = (float)(time % 1.0);
            float intensity = 0.2f;
            if (beat < 0.1f) intensity = 1.0f;
            else if (beat > 0.2f && beat < 0.3f) intensity = 0.8f;

            var baseCol = State.CurrentColor.R == 0 && State.CurrentColor.G == 0 && State.CurrentColor.B == 0
                ? new Color(255, 0, 0)
                : State.CurrentColor;

            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                colors[i] = new Color(
                    (byte)Math.Clamp((int)(baseCol.R * intensity), 0, 255),
                    (byte)Math.Clamp((int)(baseCol.G * intensity), 0, 255),
                    (byte)Math.Clamp((int)(baseCol.B * intensity), 0, 255)
                );
            }
        }
    }
}
