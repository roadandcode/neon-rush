using System.Collections.Generic;
using NUnit.Framework;
using RoadAndCode.Core.Messaging;
using RoadAndCode.NeonRush.Screens.Logic;
using RoadAndCode.NeonRush.Shared.Screens;
using RoadAndCode.NeonRush.Shared.Track;

namespace RoadAndCode.NeonRush.Screens.Tests
{
    public sealed class ImpactFlashPresenterTests
    {
        [Test]
        public void AHit_FlashesTheScreen_OncePerHit()
        {
            var view = new FakeImpactFlashView();
            var bus = new MessageBus();
            using var presenter = new ImpactFlashPresenter(view, bus);
            presenter.Start();

            bus.Publish(new HazardHit());
            bus.Publish(new PickupCollected(10));

            Assert.That(view.Flashes, Is.EqualTo(1));
        }

        [Test]
        public void AfterDispose_HitsAreIgnored()
        {
            var view = new FakeImpactFlashView();
            var bus = new MessageBus();
            var presenter = new ImpactFlashPresenter(view, bus);
            presenter.Start();
            presenter.Dispose();

            bus.Publish(new HazardHit());

            Assert.That(view.Flashes, Is.Zero);
        }
    }

    public sealed class ButtonPressPresenterTests
    {
        [Test]
        public void EveryPress_IsAnnounced_UntilThePresenterIsDisposed()
        {
            var view = new FakeButtonPressView();
            var bus = new MessageBus();
            int announced = 0;
            bus.Subscribe<ButtonPressed>(_ => announced++);
            var presenter = new ButtonPressPresenter(view, bus);
            presenter.Start();

            view.Press();
            view.Press();
            Assert.That(announced, Is.EqualTo(2));

            presenter.Dispose();
            view.Press();
            Assert.That(announced, Is.EqualTo(2));
        }
    }

    public sealed class SoundTogglePresenterTests
    {
        private FakeSoundToggleView _view;
        private MessageBus _bus;
        private FakeSoundSettings _settings;
        private SoundTogglePresenter _presenter;

        private void StartWith(bool soundOn)
        {
            _view = new FakeSoundToggleView();
            _bus = new MessageBus();
            _settings = new FakeSoundSettings(_bus, soundOn);
            _presenter = new SoundTogglePresenter(_view, _settings, _bus);
            _presenter.Start();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Start_ShowsTheSavedSetting(bool soundOn)
        {
            StartWith(soundOn);

            Assert.That(_view.SoundOn, Is.EqualTo(soundOn));
        }

        [Test]
        public void EachPress_FlipsTheSetting_AndTheControlFollows()
        {
            StartWith(soundOn: true);
            var shown = new List<bool?>();

            _view.Press();
            shown.Add(_view.SoundOn);
            _view.Press();
            shown.Add(_view.SoundOn);

            Assert.That(shown, Is.EqualTo(new bool?[] { false, true }));
            Assert.That(_settings.SoundOn, Is.True);
        }

        [Test]
        public void AChangeMadeElsewhere_ShowsOnTheControl()
        {
            StartWith(soundOn: true);

            _settings.SetSoundOn(false);

            Assert.That(_view.SoundOn, Is.False);
        }

        [Test]
        public void AfterDispose_TheControlDoesNothing()
        {
            StartWith(soundOn: true);
            _presenter.Dispose();

            _view.Press();

            Assert.That(_settings.SoundOn, Is.True);
        }
    }
}
