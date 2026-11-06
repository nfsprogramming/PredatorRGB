using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ForgeSense.Core
{
    public class ScreenSyncEngine : IDisposable
    {
        private LightingEngine _lighting;
        private CancellationTokenSource _cts;
        private Task _captureTask;

        public ScreenSyncEngine(LightingEngine lighting)
        {
            _lighting = lighting;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _captureTask = Task.Run(() => CaptureLoop(_cts.Token));
        }

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        private void CaptureLoop(CancellationToken token)
        {
            // Get actual screen resolution
            int width = GetSystemMetrics(0); // SM_CXSCREEN
            int height = GetSystemMetrics(1); // SM_CYSCREEN
            
            if (width == 0 || height == 0)
            {
                width = 1920;
                height = 1080;
            }

            while (!token.IsCancellationRequested)
            {
                // Only capture screen if Ambilight effect is active
                if (_lighting.Manager.CurrentEffect?.Id != "ambilight")
                {
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.CopyFromScreen(0, 0, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                        }

                        int zoneWidth = width / 4;
                        
                        // Sample the entire 100% of the screen
                        int sampleHeight = height;
                        int startY = 0;

                        for (int zone = 0; zone < 4; zone++)
                        {
                            int startX = zone * zoneWidth;
                            
                            // Root Mean Square averaging to favor bright colors
                            double rSquared = 0, gSquared = 0, bSquared = 0;
                            int count = 0;
                            
                            // Sample across the 100% of the screen (stride 50px for performance)
                            for (int y = startY; y < sampleHeight; y += 50)
                            {
                                for (int x = startX; x < startX + zoneWidth; x += 50)
                                {
                                    Color c = bmp.GetPixel(x, y);
                                    
                                    // RMS Averaging: Square the values to heavily favor bright colors over dark colors
                                    rSquared += (c.R * c.R);
                                    gSquared += (c.G * c.G);
                                    bSquared += (c.B * c.B);
                                    count++;
                                }
                            }

                            if (count > 0)
                            {
                                // Root of the mean of the squares
                                int rmsR = (int)Math.Sqrt(rSquared / count);
                                int rmsG = (int)Math.Sqrt(gSquared / count);
                                int rmsB = (int)Math.Sqrt(bSquared / count);
                                
                                // Saturation Boost to prevent muddy gray colors
                                float gray = (rmsR + rmsG + rmsB) / 3f;
                                float saturationBoost = 1.4f; // 40% more saturation
                                
                                int finalR = Math.Clamp((int)(gray + (rmsR - gray) * saturationBoost), 0, 255);
                                int finalG = Math.Clamp((int)(gray + (rmsG - gray) * saturationBoost), 0, 255);
                                int finalB = Math.Clamp((int)(gray + (rmsB - gray) * saturationBoost), 0, 255);
                                
                                // Smooth transition
                                var oldColor = _lighting.AmbilightEffect.ScreenColors[zone];
                                _lighting.AmbilightEffect.ScreenColors[zone] = new OpenRGB.NET.Color(
                                    (byte)((oldColor.R * 0.7f) + (finalR * 0.3f)),
                                    (byte)((oldColor.G * 0.7f) + (finalG * 0.3f)),
                                    (byte)((oldColor.B * 0.7f) + (finalB * 0.3f))
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Screen capture error: " + ex.Message);
                }

                // ~30 FPS capture limit
                Thread.Sleep(1000 / 30);
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _captureTask?.Wait();
        }
    }
}
