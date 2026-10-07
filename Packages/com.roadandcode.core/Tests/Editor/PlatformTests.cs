using NUnit.Framework;
using RoadAndCode.Core.Platforms;
using UnityEngine;

namespace RoadAndCode.Core.Tests
{
    public sealed class PlatformTests
    {
        [TestCase(RuntimePlatform.WindowsPlayer, PlatformKind.Desktop)]
        [TestCase(RuntimePlatform.OSXPlayer, PlatformKind.Desktop)]
        [TestCase(RuntimePlatform.LinuxPlayer, PlatformKind.Desktop)]
        [TestCase(RuntimePlatform.WindowsEditor, PlatformKind.Desktop)]
        [TestCase(RuntimePlatform.Android, PlatformKind.Mobile)]
        [TestCase(RuntimePlatform.IPhonePlayer, PlatformKind.Mobile)]
        [TestCase(RuntimePlatform.WebGLPlayer, PlatformKind.Web)]
        public void RuntimePlatforms_MapToTheirFamily(RuntimePlatform platform, PlatformKind expected)
        {
            Assert.That(RuntimePlatformService.KindOf(platform), Is.EqualTo(expected));
        }

        [Test]
        public void FixedPlatform_ReportsWhatItWasGiven()
        {
            Assert.That(new FixedPlatform(PlatformKind.Web).Kind, Is.EqualTo(PlatformKind.Web));
        }

        [Test]
        public void PerPlatform_ReturnsTheValueForEachFamily()
        {
            var value = new PerPlatform<string>("desktop", "mobile", "web");

            Assert.That(value.For(PlatformKind.Desktop), Is.EqualTo("desktop"));
            Assert.That(value.For(PlatformKind.Mobile), Is.EqualTo("mobile"));
            Assert.That(value.For(PlatformKind.Web), Is.EqualTo("web"));
        }
    }
}
