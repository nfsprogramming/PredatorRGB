using System;
using ForgeSense.Core.Effects;
using OpenRGB.NET;
using Color = OpenRGB.NET.Color;

namespace ForgeSense.Core
{
    public enum EngineState
    {
        IDLE,
        STARTING,
        RUNNING,
        STOPPING,
        ERROR
    }

    public class EffectManager
    {
        private readonly EffectRegistry _registry;
        private readonly ForgeSense.Core.DSP.AudioAnalyzer _audio;
        private readonly object _transitionLock = new object();
        
        public ILightingEffect CurrentEffect { get; private set; }
        public string CurrentEffectId { get; private set; }
        public long TransitionVersion { get; private set; } = 0;
        public EngineState CurrentEngineState { get; private set; } = EngineState.IDLE;

        public RgbState State { get; private set; } = new RgbState();

        public EffectManager(EffectRegistry registry, ForgeSense.Core.DSP.AudioAnalyzer audio)
        {
            _registry = registry;
            _audio = audio;
        }

        public void SetEffect(string effectId)
        {
            lock (_transitionLock)
            {
                var nextEffect = _registry.GetEffect(effectId);
                if (nextEffect == null || nextEffect == CurrentEffect) return;

                TransitionVersion++; // Invalidate stale tasks
                
                if (CurrentEffect != null)
                {
                    CurrentEngineState = EngineState.STOPPING;
                    Console.WriteLine($"[RGB STOP] {CurrentEffectId}");
                    CurrentEffect.Stop();
                }

                CurrentEngineState = EngineState.STARTING;
                CurrentEffectId = effectId;
                CurrentEffect = nextEffect;
                
                // Audio lifecycle management
                if (CurrentEffect is BaseAudioEffect)
                {
                    _audio?.SetAudioSource(ForgeSense.Core.DSP.AudioSourceType.SystemAudio);
                }
                else
                {
                    _audio?.SetAudioSource(ForgeSense.Core.DSP.AudioSourceType.Disabled);
                }
                
                // Inject preserved state
                CurrentEffect.SetState(State);
                
                Console.WriteLine($"[RGB START] {CurrentEffectId}");
                CurrentEffect.Start();
                CurrentEngineState = EngineState.RUNNING;
                Console.WriteLine($"[RGB OWNER] {CurrentEffectId} (Transition {TransitionVersion})");
            }
        }

        public void Reset()
        {
            lock (_transitionLock)
            {
                TransitionVersion++;
                if (CurrentEffect != null)
                {
                    CurrentEngineState = EngineState.STOPPING;
                    CurrentEffect.Stop();
                    CurrentEffect = null;
                    CurrentEffectId = null;
                }
                CurrentEngineState = EngineState.IDLE;
            }
        }

        public void SetState(RgbState state)
        {
            lock (_transitionLock)
            {
                State = state.Clone();
                CurrentEffect?.SetState(State);
            }
        }
        
        // Backward compatibility for basic color change
        public void SetColor(Color color, int version)
        {
            lock (_transitionLock)
            {
                State.CurrentColor = color;
                State.ColorVersion = version;
                CurrentEffect?.SetState(State);
            }
        }
        
        public void SetBrightness(float brightness)
        {
            lock (_transitionLock)
            {
                State.Brightness = Math.Clamp(brightness, 0f, 1f);
            }
        }

        public void SetSpeed(float speed)
        {
            lock (_transitionLock)
            {
                State.Speed = Math.Max(0.1f, speed);
            }
        }

        public void TriggerEvent(string eventType)
        {
            lock (_transitionLock)
            {
                CurrentEffect?.Trigger(eventType);
            }
        }
    }
}
