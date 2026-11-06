using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ForgeSense.Core.Effects;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core
{
    public class LightingEngine : IDisposable
    {
        private OpenRgbClient? _client;
        private CancellationTokenSource _cts;
        private Task _renderTask;
        private bool _isUpdatingHardware = false;
        private Color[] _lastSentColors;
        
        public EffectRegistry Registry { get; private set; }
        public HardwareCommandQueue CommandQueue { get; private set; }
        public EffectManager Manager { get; private set; }
        
        private int _deviceId = -1;
        public int LedCount { get; private set; } = 4;
        
        private AcerWmiDriver _acerDriver;
        public AcerWmiDriver AcerDriver => _acerDriver;
        private bool _hasAcerWmi = false;
        public bool HasAcerWmi => _hasAcerWmi;

        private float _accumulatedTime = 0f;
        private long _lastCycleTime = 0;
        private DateTime _liveStreamUntil = DateTime.MinValue;

        private readonly object _liveLock = new object();
        private byte[]? _pendingLiveR;
        private byte[]? _pendingLiveG;
        private byte[]? _pendingLiveB;
        private readonly AutoResetEvent _liveStreamSignal = new AutoResetEvent(false);
        private Thread? _liveHardwareThread;
        private volatile bool _isLiveWorkerRunning = true;

        public void UpdateLiveZones(byte[] r, byte[] g, byte[] b)
        {
            _liveStreamUntil = DateTime.UtcNow.AddMilliseconds(800);

            if (!_hasAcerWmi && _deviceId == -1) return;

            lock (_liveLock)
            {
                _pendingLiveR = (byte[])r.Clone();
                _pendingLiveG = (byte[])g.Clone();
                _pendingLiveB = (byte[])b.Clone();
            }
            _liveStreamSignal.Set();
        }

        private void RunLiveHardwareWorkerLoop()
        {
            while (_isLiveWorkerRunning)
            {
                _liveStreamSignal.WaitOne(100);

                byte[]? r = null;
                byte[]? g = null;
                byte[]? b = null;

                lock (_liveLock)
                {
                    if (_pendingLiveR == null) continue;
                    r = _pendingLiveR;
                    g = _pendingLiveG;
                    b = _pendingLiveB;
                    _pendingLiveR = null;
                }

                if (r == null || g == null || b == null) continue;

                try
                {
                    if (_hasAcerWmi && _acerDriver.IsAvailable)
                    {
                        _acerDriver.SetAllZones(r, g, b);
                    }

                    if (_deviceId != -1 && _client != null)
                    {
                        try
                        {
                            var hwColors = new Color[r.Length];
                            for (int i = 0; i < r.Length; i++)
                                hwColors[i] = new Color(r[i], g[i], b[i]);
                            _client.UpdateLeds(_deviceId, hwColors);
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LightingEngine] Live stream error: " + ex.Message);
                }
            }
        }

        // References for other engines to push data
        public AmbilightEffect AmbilightEffect { get; private set; }
        public ForgeSense.Core.DSP.AudioAnalyzer AudioAnalyzer { get; private set; }

        public LightingEngine(TelemetryEngine telemetry, ReactiveEngine reactive, ForgeSense.Core.DSP.AudioAnalyzer audio)
        {
            CommandQueue = new HardwareCommandQueue();
            Registry = new EffectRegistry();
            Manager = new EffectManager(Registry, audio);
            AudioAnalyzer = audio;

            _acerDriver = new AcerWmiDriver();
            try
            {
                if (_acerDriver.Initialize())
                {
                    _hasAcerWmi = true;
                    LedCount = 4;
                    Console.WriteLine("[LightingEngine] Native Acer Predator WMI keyboard hardware detected and active.");
                }

                _liveHardwareThread = new Thread(RunLiveHardwareWorkerLoop)
                {
                    IsBackground = true,
                    Name = "LiveHardwareWorker",
                    Priority = ThreadPriority.AboveNormal
                };
                _liveHardwareThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LightingEngine] Acer WMI driver init error: " + ex.Message);
            }

            try
            {
                _client = new OpenRgbClient(name: "ForgeSense", autoConnect: false);
                try
                {
                    _client.Connect();
                    var count = _client.GetControllerCount();
                    for (int i = 0; i < count; i++)
                    {
                        var device = _client.GetControllerData(i);
                        if (device.Name.Contains("Acer", StringComparison.OrdinalIgnoreCase) ||
                            device.Name.Contains("Predator", StringComparison.OrdinalIgnoreCase) ||
                            device.Name.Contains("Keyboard", StringComparison.OrdinalIgnoreCase))
                        {
                            _deviceId = i;
                            LedCount = device.Leds.Length;

                            for (int m = 0; m < device.Modes.Length; m++)
                            {
                                if (device.Modes[m].Name.Contains("Direct", StringComparison.OrdinalIgnoreCase) ||
                                    device.Modes[m].Name.Contains("Custom", StringComparison.OrdinalIgnoreCase))
                                {
                                    _client.UpdateMode(_deviceId, m);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[LightingEngine] OpenRGB server not running (using native Acer WMI driver): " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LightingEngine] OpenRGB client unavailable: " + ex.Message);
                _client = null;
            }
            
            // If device not found, default to 4 for simulation
            if (LedCount == 0) LedCount = 4;

            // Register all effects (1-10)
            Registry.Register(new StaticEffect());
            Registry.Register(new DualColorEffect());
            Registry.Register(new TripleColorEffect());
            Registry.Register(new QuadColorEffect());
            Registry.Register(new GradientEffect());
            Registry.Register(new ColorCycleEffect());
            Registry.Register(new RandomPaletteEffect());
            Registry.Register(new BreathingEffect());
            Registry.Register(new SmoothFadeEffect());
            Registry.Register(new PulseEffect());

            // Register advanced effects (11-20)
            Registry.Register(new SpectrumFlowEffect());
            Registry.Register(new ColorWaveEffect());
            Registry.Register(new PlasmaEffect());
            Registry.Register(new AuroraEffect());
            Registry.Register(new NebulaEffect());
            Registry.Register(new LiquidEffect());
            Registry.Register(new EnergyFlowEffect());
            Registry.Register(new PrismEffect());
            Registry.Register(new VortexEffect());
            Registry.Register(new ColorDriftEffect());

            // Register wave effects (21-30)
            Registry.Register(new RainbowWaveEffect());
            Registry.Register(new ReverseRainbowEffect());
            Registry.Register(new ColorMorphEffect());
            Registry.Register(new DualWaveEffect());
            Registry.Register(new OppositeWaveEffect());
            Registry.Register(new CenterOutEffect());
            Registry.Register(new OutsideInEffect());
            Registry.Register(new ZoneChaseEffect());
            Registry.Register(new ZoneScannerEffect());
            Registry.Register(new RgbRadarEffect());

            // Register nature/atmospheric effects (31-40)
            Registry.Register(new EnergyRippleEffect());
            Registry.Register(new FireEffect());
            Registry.Register(new LavaEffect());
            Registry.Register(new WaterEffect());
            Registry.Register(new OceanEffect());
            Registry.Register(new LightningStormEffect());
            Registry.Register(new AuroraSkyEffect());
            Registry.Register(new SunsetEffect());
            Registry.Register(new MeteorShowerEffect());
            Registry.Register(new GalaxyEffect());

            // Register cinematic/cyber effects (41-50)
            Registry.Register(new IceEffect());
            Registry.Register(new ThunderEffect());
            Registry.Register(new ElectricStormEffect());
            Registry.Register(new NeonFluxEffect());
            Registry.Register(new SynthwaveEffect());
            Registry.Register(new MatrixEffect());
            Registry.Register(new DigitalGlitchEffect());
            Registry.Register(new EnergyCoreEffect());
            Registry.Register(new PowerSurgeEffect());
            Registry.Register(new CyberGridEffect());

            // Register gaming effects (51-60)
            Registry.Register(new RageEffect());
            Registry.Register(new CriticalHitEffect());
            Registry.Register(new DamageFlashEffect());
            Registry.Register(new HealthPulseEffect());
            Registry.Register(new ComboEffect());
            Registry.Register(new LevelUpEffect());
            Registry.Register(new VictoryEffect());
            Registry.Register(new DefeatEffect());
            Registry.Register(new RespawnEffect());
            Registry.Register(new GameOverEffect());

            // Register sci-fi effects (61-70)
            Registry.Register(new HologramEffect());
            Registry.Register(new CyberPulseEffect());
            Registry.Register(new DigitalRainEffect());
            Registry.Register(new QuantumFlowEffect());
            Registry.Register(new SingularityEffect());
            Registry.Register(new BlackHoleEffect());
            Registry.Register(new WormholeEffect());
            Registry.Register(new TimeWarpEffect());
            Registry.Register(new HolographicSpectrumEffect());
            Registry.Register(new CyberPulseWaveEffect());

            // Register reactive effects (71-80)
            Registry.Register(new KeyReactiveEffect());
            Registry.Register(new ZoneReactiveEffect());
            Registry.Register(new RippleReactiveEffect());
            Registry.Register(new WaveReactiveEffect());
            Registry.Register(new KeyTrailEffect());
            Registry.Register(new ReactiveBurstEffect());
            Registry.Register(new ReactiveSpectrumEffect());
            Registry.Register(new ReactivePulseEffect());
            Registry.Register(new ReactiveShockwaveEffect());
            Registry.Register(new ReactivePlasmaEffect());

            // Register audio effects (81-90)
            Registry.Register(new AudioPulseEffect());
            Registry.Register(new BeatFlashEffect());
            Registry.Register(new BassReactorEffect());
            Registry.Register(new SpectrumWaveEffect());
            Registry.Register(new AudioVortexEffect());
            Registry.Register(new BassTunnelEffect());
            Registry.Register(new BeatRippleEffect());
            Registry.Register(new FrequencyFlowEffect());
            Registry.Register(new MusicAuroraEffect());
            Registry.Register(new AudioPlasmaEffect());

            // Register signature effects (91-100)
            Registry.Register(new FirestormEffect());
            Registry.Register(new OceanPulseEffect());
            Registry.Register(new AuroraBorealisEffect());
            Registry.Register(new CosmicDustEffect());
            Registry.Register(new SolarFlareEffect());
            Registry.Register(new VolcanicCoreEffect());
            Registry.Register(new GhostModeEffect());
            Registry.Register(new CyberSamuraiEffect());
            Registry.Register(new PredatorVisionEffect());
            Registry.Register(new UltimateFusionEffect());

            // Initialize all
            var allEffectIds = new[] { 
                "static", "dual_color", "triple_color", "quad_color", "gradient", "color_cycle", "random_palette", "breathing", "smooth_fade", "pulse",
                "spectrum_flow", "color_wave", "plasma", "aurora", "nebula", "liquid", "energy_flow", "prism", "vortex", "color_drift",
                "rainbow_wave", "reverse_rainbow", "color_morph", "dual_wave", "opposite_wave", "center_out", "outside_in", "zone_chase", "zone_scanner", "rgb_radar",
                "energy_ripple", "fire", "lava", "water", "ocean", "lightning_storm", "aurora_sky", "sunset", "meteor_shower", "galaxy",
                "ice", "thunder", "electric_storm", "neon_flux", "synthwave", "matrix", "digital_glitch", "energy_core", "power_surge", "cyber_grid",
                "rage", "critical_hit", "damage_flash", "health_pulse", "combo", "level_up", "victory", "defeat", "respawn", "game_over",
                "hologram", "cyber_pulse", "digital_rain", "quantum_flow", "singularity", "black_hole", "wormhole", "time_warp", "holographic_spectrum", "cyber_pulse_wave",
                "key_reactive", "zone_reactive", "ripple_reactive", "wave_reactive", "key_trail", "reactive_burst", "reactive_spectrum", "reactive_pulse", "reactive_shockwave", "reactive_plasma",
                "audio_pulse", "beat_flash", "bass_reactor", "spectrum_wave", "audio_vortex", "bass_tunnel", "beat_ripple", "frequency_flow", "music_aurora", "audio_plasma",
                "firestorm", "ocean_pulse", "aurora_borealis", "cosmic_dust", "solar_flare", "volcanic_core", "ghost_mode", "cyber_samurai", "predator_vision", "ultimate_fusion"
            };

            foreach (var effectId in allEffectIds)
            {
                var effect = Registry.GetEffect(effectId);
                if (effect != null)
                {
                    effect.Initialize(LedCount);
                }
            }
            
            // Set default effect
            Manager.SetEffect("static");
        }

        public void Start()
        {
            if (_deviceId == -1) 
            {
                Console.WriteLine("[LightingEngine] No compatible device found. Running in simulation mode.");
            }

            _cts = new CancellationTokenSource();
            _renderTask = Task.Run(() => RenderLoop(_cts.Token));
        }

        private void RenderLoop(CancellationToken token)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int targetDelay = 1000 / 60; // 60 FPS strict animation target
            _lastCycleTime = 0;

            while (!token.IsCancellationRequested)
            {
                var cycleTime = stopwatch.ElapsedMilliseconds;
                float deltaTime = (cycleTime - _lastCycleTime) / 1000.0f;
                _lastCycleTime = cycleTime;
                
                float currentSpeed = Manager.State.Speed;
                _accumulatedTime += deltaTime * currentSpeed;

                // Process safe synchronous command queue (colors, modes, settings)
                CommandQueue.ProcessQueue();

                // If live stream from UI preview is actively streaming, yield hardware control to UI stream
                if (DateTime.UtcNow < _liveStreamUntil)
                {
                    int elapsedLive = (int)(stopwatch.ElapsedMilliseconds - cycleTime);
                    int delayLive = targetDelay - elapsedLive;
                    if (delayLive > 0) Thread.Sleep(delayLive);
                    continue;
                }

                try
                {
                    var colors = new Color[LedCount];
                    
                    var effect = Manager.CurrentEffect;
                    var version = Manager.TransitionVersion;
                    
                    if (effect != null)
                    {
                        if (effect is BaseAudioEffect audioEffect)
                        {
                            // Snapshot safely
                            lock (AudioAnalyzer.Features)
                            {
                                audioEffect.AudioData.CopyFrom(AudioAnalyzer.Features);
                            }
                        }
                        effect.Update(_accumulatedTime, deltaTime, out colors);
                    }
                    
                    // Apply Master Brightness
                    float currentBrightness = Manager.State.Brightness;
                    for (int i = 0; i < LedCount; i++)
                    {
                        colors[i] = new Color(
                            (byte)(colors[i].R * currentBrightness),
                            (byte)(colors[i].G * currentBrightness),
                            (byte)(colors[i].B * currentBrightness)
                        );
                    }

                    // Send hardware update if not busy
                    if ((_hasAcerWmi || _deviceId != -1) && !_isUpdatingHardware)
                    {
                        // Transition safety check: drop frame if the version changed mid-render
                        // AND only send if we are actively RUNNING
                        if (version == Manager.TransitionVersion && Manager.CurrentEngineState == EngineState.RUNNING)
                        {
                            bool isDirty = _lastSentColors == null || _lastSentColors.Length != LedCount;
                            if (!isDirty)
                            {
                                for (int i = 0; i < LedCount; i++)
                                {
                                    if (colors[i].R != _lastSentColors[i].R || 
                                        colors[i].G != _lastSentColors[i].G || 
                                        colors[i].B != _lastSentColors[i].B)
                                    {
                                        isDirty = true;
                                        break;
                                    }
                                }
                            }
                            
                            if (isDirty)
                            {
                                _isUpdatingHardware = true;
                                var hwColors = (Color[])colors.Clone();
                                _lastSentColors = hwColors; // Update cache
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        if (_hasAcerWmi && _acerDriver.IsAvailable)
                                        {
                                            byte[] r = new byte[LedCount];
                                            byte[] g = new byte[LedCount];
                                            byte[] b = new byte[LedCount];
                                            for (int i = 0; i < LedCount; i++)
                                            {
                                                r[i] = hwColors[i].R;
                                                g[i] = hwColors[i].G;
                                                b[i] = hwColors[i].B;
                                            }
                                            _acerDriver.SetAllZones(r, g, b);
                                        }

                                        if (_deviceId != -1 && _client != null)
                                        {
                                            try { _client.UpdateLeds(_deviceId, hwColors); } catch { }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Hardware update error: " + ex.Message);
                                    }
                                    finally
                                    {
                                        _isUpdatingHardware = false;
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Render loop error: " + ex.Message);
                }

                int elapsed = (int)(stopwatch.ElapsedMilliseconds - cycleTime);
                int delay = targetDelay - elapsed;
                if (delay > 0) Thread.Sleep(delay);
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _renderTask?.Wait();
            Manager.CurrentEffect?.Stop();
            Registry.DisposeAll();
            _acerDriver?.Dispose();
            _client?.Dispose();
        }
    }
}
