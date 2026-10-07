using RoadAndCode.NeonRush.Sound.Logic;
using RoadAndCode.NeonRush.Sound.Presentation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Sound.Composition
{
    /// <summary>
    /// Registers sound effects, music and the sound setting. Needs an IGameFlow, an ISaveStore and
    /// the message bus from the scope. Provides the ISoundSettings that screens show and change.
    /// Take this installer out and the game runs silent: nothing else refers to audio.
    /// </summary>
    public sealed class SoundInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private SoundOutput _output;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_output.Bank.Mix);
            builder.RegisterComponent(_output).As<ISoundPlayer, IMusicPlayer, IMasterVolume>();

            // Entry points are registered under every interface they implement, which is how
            // SoundSettings also becomes the scope's ISoundSettings.
            builder.RegisterEntryPoint<SoundSettings>();
            builder.RegisterEntryPoint<SoundDirector>();
            builder.RegisterEntryPoint<MusicDirector>();
        }

        private void OnValidate()
        {
            if (_output == null) Debug.LogError($"{nameof(SoundInstaller)} on '{name}' has no sound output.", this);
        }
    }
}
