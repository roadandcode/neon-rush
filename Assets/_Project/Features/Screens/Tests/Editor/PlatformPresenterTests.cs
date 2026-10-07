using NUnit.Framework;
using RoadAndCode.Core.Platforms;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Input;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class ControlHintsPresenterTests
    {
        [Test]
        public void HintsAreShown_ForEachSchemeTheProfileUses_AndNoOther()
        {
            var view = new FakeControlHintsView();
            var profile = new FakeInputProfile(InputNames.Schemes.Pointer, InputNames.Schemes.Gamepad);

            new ControlHintsPresenter(view, profile).Start();

            Assert.That(view.Shown, Is.EquivalentTo(new[] { InputNames.Schemes.Pointer, InputNames.Schemes.Gamepad }));
        }

        [Test]
        public void BeforeStart_NothingIsShown()
        {
            var view = new FakeControlHintsView();

            _ = new ControlHintsPresenter(view, new FakeInputProfile(InputNames.Schemes.Keyboard));

            Assert.That(view.Shown, Is.Empty);
        }
    }

    public sealed class SafeAreaPresenterTests
    {
        private static readonly ScreenInsets NotchLeft = new ScreenInsets(0.05f, 0f, 0f, 0.02f);
        private static readonly ScreenInsets NotchRight = new ScreenInsets(0f, 0.05f, 0f, 0.02f);

        private FakeSafeAreaView _view;
        private FakeSafeArea _safeArea;
        private SafeAreaPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeSafeAreaView();
            _safeArea = new FakeSafeArea { Insets = NotchLeft };
            _presenter = new SafeAreaPresenter(_view, _safeArea);
        }

        [Test]
        public void Start_AppliesTheCurrentInsets()
        {
            _presenter.Start();

            Assert.That(_view.Applied, Is.EqualTo(new[] { NotchLeft }));
        }

        [Test]
        public void WhileNothingChanges_NothingIsApplied()
        {
            _presenter.Start();

            for (int i = 0; i < 10; i++) _presenter.Tick();

            Assert.That(_view.Applied, Has.Count.EqualTo(1));
        }

        [Test]
        public void TurningThePhoneOver_MovesThePaddingToTheOtherSide()
        {
            _presenter.Start();

            _safeArea.Insets = NotchRight;
            _presenter.Tick();
            _presenter.Tick();

            Assert.That(_view.Applied, Is.EqualTo(new[] { NotchLeft, NotchRight }));
        }

        [Test]
        public void WhenTheUiIsResized_TheSameInsetsAreAppliedAgain()
        {
            _presenter.Start();

            _view.Resize();

            Assert.That(_view.Applied, Is.EqualTo(new[] { NotchLeft, NotchLeft }));
        }

        [Test]
        public void AfterDispose_ResizesAreIgnored()
        {
            _presenter.Start();
            _presenter.Dispose();

            _view.Resize();

            Assert.That(_view.Applied, Has.Count.EqualTo(1));
        }
    }
}
