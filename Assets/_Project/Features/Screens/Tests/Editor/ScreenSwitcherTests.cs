using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Flow;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    /// <summary>Runs the switcher against the real screen table, so these are also the table's tests.</summary>
    public sealed class ScreenSwitcherTests
    {
        private FakeScreen _menu;
        private FakeScreen _hud;
        private FakeScreen _pause;
        private FakeScreen _gameOver;
        private FakeFlow _flow;
        private MessageBus _bus;
        private ScreenSwitcher _switcher;

        [SetUp]
        public void SetUp()
        {
            _menu = new FakeScreen();
            _hud = new FakeScreen();
            _pause = new FakeScreen();
            _gameOver = new FakeScreen();
            _flow = new FakeFlow();
            _bus = new MessageBus();
            _switcher = new ScreenSwitcher(ScreenTable.ForGame(_menu, _hud, _pause, _gameOver), _flow, _bus);
        }

        private void Enter(GamePhase phase)
        {
            GamePhase previous = _flow.Phase;
            _flow.Phase = phase;
            _bus.Publish(new GamePhaseChanged(previous, phase));
        }

        [TestCase(GamePhase.Boot, false, false, false, false)]
        [TestCase(GamePhase.Menu, true, false, false, false)]
        [TestCase(GamePhase.Run, false, true, false, false)]
        [TestCase(GamePhase.Paused, false, true, true, false)]
        [TestCase(GamePhase.GameOver, false, false, false, true)]
        public void EachPhase_ShowsItsScreens_AndHidesTheRest(GamePhase phase, bool menu, bool hud, bool pause, bool gameOver)
        {
            _switcher.Start();

            Enter(phase);

            Assert.That(_menu.Visible, Is.EqualTo(menu), "menu");
            Assert.That(_hud.Visible, Is.EqualTo(hud), "hud");
            Assert.That(_pause.Visible, Is.EqualTo(pause), "pause");
            Assert.That(_gameOver.Visible, Is.EqualTo(gameOver), "game over");
        }

        [Test]
        public void Start_CatchesUpWithAPhaseThatWasEnteredEarlier()
        {
            _flow.Phase = GamePhase.Run;

            _switcher.Start();

            Assert.That(_hud.Visible, Is.True);
            Assert.That(_menu.Visible, Is.False);
        }

        [Test]
        public void BeforeStart_NoScreenIsTouched()
        {
            Enter(GamePhase.Run);

            Assert.That(_menu.Visible, Is.Null);
            Assert.That(_hud.Visible, Is.Null);
        }

        [Test]
        public void AfterDispose_PhaseChangesAreIgnored()
        {
            _switcher.Start();
            _switcher.Dispose();

            Enter(GamePhase.GameOver);

            Assert.That(_gameOver.Visible, Is.False);
        }
    }
}
