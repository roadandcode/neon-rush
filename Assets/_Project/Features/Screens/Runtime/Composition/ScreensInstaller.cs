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
    /// IInputProfile, an ISafeArea and the message bus from the scope. Provides the IPointerClaims
    /// that keeps gestures from starting on on-screen controls.
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

            builder.RegisterInstance(ScreenTable.ForGame(_document.Menu, _document.Hud, _document.Pause, _document.GameOver));
            builder.RegisterEntryPoint<ScreenSwitcher>();

            builder.RegisterEntryPoint<MenuPresenter>();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<GameOverPresenter>();
            builder.RegisterEntryPoint<ControlHintsPresenter>();
            builder.RegisterEntryPoint<SafeAreaPresenter>();
        }

        private void OnValidate()
        {
            if (_document == null) Debug.LogError($"{nameof(ScreensInstaller)} on '{name}' has no screens document.", this);
        }
    }
}
