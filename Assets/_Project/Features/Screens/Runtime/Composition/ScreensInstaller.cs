using RoadAndCode.NeonRush.Screens.Input;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Screens.Presentation;
using RoadAndCode.NeonRush.Shared.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace RoadAndCode.NeonRush.Screens.Composition
{
    /// <summary>
    /// Registers the menu, HUD, pause and game-over screens. Needs an IGameFlow, an IBestScore, an
    /// IInputProfile, an ISafeArea, an ISoundSettings, the InputActionAsset and the message bus from the scope. Provides
    /// the IPointerClaims that keeps gestures from starting on on-screen controls.
    /// </summary>
    public sealed class ScreensInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private ScreensDocument _document;

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance<IMenuView>(_document.Menu);
            builder.RegisterInstance<IHudView>(_document.Hud);
            builder.RegisterInstance<IPauseView>(_document.Pause);
            builder.RegisterInstance<IGameOverView>(_document.GameOver);
            builder.RegisterInstance<IControlHintsView>(_document.ControlHints);
            builder.RegisterInstance<ISafeAreaView>(_document.SafeArea);
            builder.RegisterInstance<IPointerClaims>(_document.PointerClaims);
            builder.RegisterInstance<IImpactFlashView>(_document.ImpactFlash);
            builder.RegisterInstance<IButtonPressView>(_document.ButtonPresses);
            builder.RegisterInstance<ISoundToggleView>(_document.SoundToggle);

            builder.RegisterInstance(ScreenTable.ForGame(_document.Menu, _document.Hud, _document.Pause, _document.GameOver));
            builder.RegisterEntryPoint<ScreenSwitcher>();

            builder.RegisterEntryPoint<MenuPresenter>();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<GameOverPresenter>();
            builder.RegisterEntryPoint<ControlHintsPresenter>();
            builder.RegisterEntryPoint<SafeAreaPresenter>();
            builder.RegisterEntryPoint<ImpactFlashPresenter>();
            builder.RegisterEntryPoint<ButtonPressPresenter>();
            builder.RegisterEntryPoint<SoundTogglePresenter>();

            // Keys and pads reach the screens as "previous", "next" and "submit", from the same masked
            // actions asset as the rest of the game's input.
            builder.Register<IMenuInput, MenuInputSource>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuNavigator>();
        }

        private void OnValidate()
        {
            if (_document == null) Debug.LogError($"{nameof(ScreensInstaller)} on '{name}' has no screens document.", this);
        }
    }
}
