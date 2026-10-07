using NUnit.Framework;
using RoadAndCode.NeonRush.Player.Input;
using RoadAndCode.NeonRush.Player.Logic;

namespace RoadAndCode.NeonRush.Player.Tests
{
    public sealed class ActionQueueTests
    {
        [Test]
        public void Actions_ComeOutInTheOrderTheyWentIn()
        {
            var queue = new ActionQueue();
            queue.Enqueue(PlayerAction.Jump);
            queue.Enqueue(PlayerAction.MoveLeft);

            Assert.That(queue.TryDequeue(out var first), Is.True);
            Assert.That(queue.TryDequeue(out var second), Is.True);
            Assert.That(queue.TryDequeue(out _), Is.False);
            Assert.That(new[] { first, second }, Is.EqualTo(new[] { PlayerAction.Jump, PlayerAction.MoveLeft }));
        }

        [Test]
        public void AFullQueue_DropsNewActions_AndKeepsTheOldOnes()
        {
            var queue = new ActionQueue();
            int accepted = 0;
            while (queue.Enqueue(PlayerAction.Jump)) accepted++;

            Assert.That(queue.Enqueue(PlayerAction.Slide), Is.False);
            Assert.That(queue.Count, Is.EqualTo(accepted));
            Assert.That(queue.TryDequeue(out var oldest), Is.True);
            Assert.That(oldest, Is.EqualTo(PlayerAction.Jump));
        }

        [Test]
        public void TheQueue_KeepsWorkingAsItWrapsAround()
        {
            var queue = new ActionQueue();

            for (int i = 0; i < 50; i++)
            {
                var action = i % 2 == 0 ? PlayerAction.MoveLeft : PlayerAction.MoveRight;
                Assert.That(queue.Enqueue(action), Is.True);
                Assert.That(queue.TryDequeue(out var back), Is.True);
                Assert.That(back, Is.EqualTo(action));
            }
        }

        [Test]
        public void Clear_EmptiesTheQueue()
        {
            var queue = new ActionQueue();
            queue.Enqueue(PlayerAction.Jump);

            queue.Clear();

            Assert.That(queue.TryDequeue(out _), Is.False);
        }
    }
}
