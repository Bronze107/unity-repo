using NUnit.Framework;

namespace Game.Events.Tests
{
    public sealed class EventBusTests
    {
        private struct TestEvent : IGameEvent
        {
            public int Value;

            public TestEvent(int value)
            {
                Value = value;
            }
        }

        [Test]
        public void Subscribe_DeduplicatesSameOwnerAndCallback()
        {
            var bus = new EventBus();
            var owner = new object();
            int callCount = 0;

            void OnEvent(TestEvent evt)
            {
                callCount++;
            }

            bus.Subscribe(owner, OnEvent);
            bus.Subscribe(owner, OnEvent);

            bus.Publish(new TestEvent(1));

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void SubscribeDuringPublish_StartsNextPublish()
        {
            var bus = new EventBus();
            var firstOwner = new object();
            var secondOwner = new object();
            int firstCallCount = 0;
            int secondCallCount = 0;

            void SecondListener(TestEvent evt)
            {
                secondCallCount++;
            }

            void FirstListener(TestEvent evt)
            {
                firstCallCount++;
                bus.Subscribe(secondOwner, SecondListener);
            }

            bus.Subscribe(firstOwner, FirstListener);

            bus.Publish(new TestEvent(1));
            bus.Publish(new TestEvent(2));

            Assert.That(firstCallCount, Is.EqualTo(2));
            Assert.That(secondCallCount, Is.EqualTo(1));
        }

        [Test]
        public void UnsubscribeDuringPublish_PreventsLaterCallbacksInSamePublish()
        {
            var bus = new EventBus();
            var firstOwner = new object();
            var secondOwner = new object();
            int secondCallCount = 0;

            void SecondListener(TestEvent evt)
            {
                secondCallCount++;
            }

            void FirstListener(TestEvent evt)
            {
                bus.Unsubscribe(secondOwner, SecondListener);
            }

            bus.Subscribe(firstOwner, FirstListener);
            bus.Subscribe(secondOwner, SecondListener);

            bus.Publish(new TestEvent(1));

            Assert.That(secondCallCount, Is.EqualTo(0));
        }

        [Test]
        public void RemoveOwnerDuringPublish_PreventsLaterCallbacksForThatOwner()
        {
            var bus = new EventBus();
            var removerOwner = new object();
            var targetOwner = new object();
            int targetCallCount = 0;

            void TargetListener(TestEvent evt)
            {
                targetCallCount++;
            }

            void RemovingListener(TestEvent evt)
            {
                bus.RemoveOwner(targetOwner);
            }

            bus.Subscribe(removerOwner, RemovingListener);
            bus.Subscribe(targetOwner, TargetListener);

            bus.Publish(new TestEvent(1));

            Assert.That(targetCallCount, Is.EqualTo(0));
        }

        [Test]
        public void EventBinding_DisposeUnsubscribesSingleListener()
        {
            var bus = new EventBus();
            var owner = new object();
            int callCount = 0;

            void OnEvent(TestEvent evt)
            {
                callCount++;
            }

            var binding = bus.Subscribe(owner, OnEvent);
            binding.Dispose();

            bus.Publish(new TestEvent(1));

            Assert.That(callCount, Is.EqualTo(0));
        }
    }
}
