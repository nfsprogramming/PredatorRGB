using ForgeSense.Core.DSP;

namespace ForgeSense.Core.Effects
{
    public abstract class BaseAudioEffect : BaseEffect
    {
        // This will be populated by the LightingEngine every frame
        // before Update() is called, so the audio effect has safe access 
        // to a snapshot of the current smoothed audio features.
        public AudioFeatureState AudioData { get; set; } = new AudioFeatureState();
    }
}
