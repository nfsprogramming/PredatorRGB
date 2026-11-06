using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core
{
    public class RgbState
    {
        public Color CurrentColor { get; set; } = new Color(255, 0, 0); // Alias to Color1 for compatibility
        public Color Color { get => CurrentColor; set => CurrentColor = value; }
        public Color Color1 { get => CurrentColor; set => CurrentColor = value; }
        public Color Color2 { get; set; } = new Color(0, 255, 0);
        public Color Color3 { get; set; } = new Color(0, 0, 255);
        public Color Color4 { get; set; } = new Color(255, 255, 0);
        
        public int ColorVersion { get; set; } = 0;
        
        public float Brightness { get; set; } = 1.0f;
        public float MinBrightness { get; set; } = 0.2f;
        public float MaxBrightness { get; set; } = 1.0f;
        
        public float Speed { get; set; } = 1.0f;
        public int PaletteSize { get; set; } = 4;
        public float Randomness { get; set; } = 0.5f;

        // Advanced Effects (11-20) properties
        public int Direction { get; set; } = 1; // 1 = Left to Right, -1 = Right to Left
        public float Saturation { get; set; } = 1.0f;
        public float Scale { get; set; } = 1.0f;
        public float Flow { get; set; } = 1.0f;
        public float Density { get; set; } = 1.0f;
        public float Intensity { get; set; } = 1.0f;
        public float Viscosity { get; set; } = 1.0f;
        public float Width { get; set; } = 1.0f; // Used for Wave Width, Energy Width, Spectrum Width
        public float Rotation { get; set; } = 1.0f;
        public float Smoothness { get; set; } = 1.0f;
        public float Trail { get; set; } = 0.5f;
        
        // Gaming/Combat properties
        public float HealthPercent { get; set; } = 100f; // 0 to 100
        public int ComboCount { get; set; } = 0;

        public RgbState Clone()
        {
            return new RgbState
            {
                CurrentColor = this.CurrentColor,
                Color2 = this.Color2,
                Color3 = this.Color3,
                Color4 = this.Color4,
                ColorVersion = this.ColorVersion,
                Brightness = this.Brightness,
                MinBrightness = this.MinBrightness,
                MaxBrightness = this.MaxBrightness,
                Speed = this.Speed,
                PaletteSize = this.PaletteSize,
                Randomness = this.Randomness,
                Direction = this.Direction,
                Saturation = this.Saturation,
                Scale = this.Scale,
                Flow = this.Flow,
                Density = this.Density,
                Intensity = this.Intensity,
                Viscosity = this.Viscosity,
                Width = this.Width,
                Rotation = this.Rotation,
                Smoothness = this.Smoothness,
                Trail = this.Trail,
                HealthPercent = this.HealthPercent,
                ComboCount = this.ComboCount
            };
        }
    }
}
