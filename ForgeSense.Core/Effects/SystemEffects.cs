using System;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core.Effects
{
    public class CpuTempEffect : BaseEffect
    {
        public override string Id => "cputemp";
        public override string DisplayName => "CPU Temp";

        private TelemetryEngine _telemetry;

        public CpuTempEffect(TelemetryEngine telemetry)
        {
            _telemetry = telemetry;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = GetTemperatureColors(_telemetry.CpuTemperature);
        }

        private Color[] GetTemperatureColors(float temp)
        {
            byte r = 0, g = 0, b = 0;
            if (temp < 50) { r = 0; g = (byte)(temp * 2); b = 255; }
            else if (temp < 75) { r = 255; g = 255; b = 0; }
            else { r = 255; g = 0; b = 0; }
            var colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = new Color(r, g, b);
            return colors;
        }
    }

    public class GpuTempEffect : BaseEffect
    {
        public override string Id => "gputemp";
        public override string DisplayName => "GPU Temp";

        private TelemetryEngine _telemetry;

        public GpuTempEffect(TelemetryEngine telemetry)
        {
            _telemetry = telemetry;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            byte r = 0, g = 0, b = 0;
            float temp = _telemetry.GpuTemperature;
            if (temp < 50) { r = 0; g = (byte)(temp * 2); b = 255; }
            else if (temp < 75) { r = 255; g = 255; b = 0; }
            else { r = 255; g = 0; b = 0; }
            
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = new Color(r, g, b);
        }
    }

    public class BatteryEffect : BaseEffect
    {
        public override string Id => "battery";
        public override string DisplayName => "Battery";

        private TelemetryEngine _telemetry;

        public BatteryEffect(TelemetryEngine telemetry)
        {
            _telemetry = telemetry;
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            Color batColor;
            if (_telemetry.BatteryLevel > 50) batColor = new Color(0, 255, 0);
            else if (_telemetry.BatteryLevel > 20) batColor = new Color(255, 255, 0);
            else 
            {
                float flash = (float)(Math.Sin(time * 10) + 1) / 2.0f;
                batColor = new Color((byte)(255 * flash), 0, 0);
            }
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++) colors[i] = batColor;
        }
    }

    public class AmbilightEffect : BaseEffect
    {
        public override string Id => "ambilight";
        public override string DisplayName => "Ambilight";

        public Color[] ScreenColors { get; set; }

        public override void Initialize(int ledCount)
        {
            base.Initialize(ledCount);
            ScreenColors = new Color[ledCount];
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            Array.Copy(ScreenColors, colors, LedCount);
        }
    }

    public class MusicSyncEffect : BaseEffect
    {
        public override string Id => "musicsync";
        public override string DisplayName => "Music Sync";

        public float[] AudioPeaks { get; set; }

        public override void Initialize(int ledCount)
        {
            base.Initialize(ledCount);
            AudioPeaks = new float[ledCount];
        }

        public override void Update(float time, float deltaTime, out Color[] colors)
        {
            colors = new Color[LedCount];
            for (int i = 0; i < LedCount; i++)
            {
                float currentVol = AudioPeaks[i]; 
                int virtualZone = (int)(i / (LedCount / 4.0f));
                virtualZone = Math.Clamp(virtualZone, 0, 3);
                
                int distFromCenter = (virtualZone == 1 || virtualZone == 2) ? 0 : 1;
                
                float r = 0, g = 100, b = 255;
                
                if (currentVol > 0.15f && distFromCenter == 0) { r = 255; g = 20; b = 147; }
                if (currentVol > 0.40f && distFromCenter == 1) { r = 255; g = 20; b = 147; }
                if (currentVol > 0.70f && distFromCenter == 0) { r = 255; g = 220; b = 50; }
                if (currentVol > 0.90f && distFromCenter == 1) { r = 255; g = 220; b = 50; }
                
                float brightness = 0.15f + (currentVol * 0.85f);
                colors[i] = new Color((byte)(r * brightness), (byte)(g * brightness), (byte)(b * brightness));
            }
        }
    }
}
