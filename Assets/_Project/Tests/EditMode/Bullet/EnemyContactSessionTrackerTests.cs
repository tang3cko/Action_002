using System.Collections.Generic;
using NUnit.Framework;
using Action002.Bullet.Logic;

namespace Action002.Tests.Bullet
{
    public class EnemyContactSessionTrackerTests
    {
        private EnemyContactSessionTracker tracker;

        [SetUp]
        public void SetUp()
        {
            tracker = new EnemyContactSessionTracker();
        }

        [Test]
        public void UpdateContacts_FirstContact_ReturnsAsNew()
        {
            var result = tracker.UpdateContacts(new List<int> { 1, 2 });
            Assert.That(result, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void UpdateContacts_ContinuedContact_NotReturnedAsNew()
        {
            tracker.UpdateContacts(new List<int> { 1 });
            var result = tracker.UpdateContacts(new List<int> { 1 });
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void UpdateContacts_LeaveAndReenter_ReturnedAsNew()
        {
            tracker.UpdateContacts(new List<int> { 1 });
            tracker.UpdateContacts(new List<int>()); // left
            var result = tracker.UpdateContacts(new List<int> { 1 });
            Assert.That(result, Is.EquivalentTo(new[] { 1 }));
        }

        [Test]
        public void UpdateContacts_EmptyList_ReturnsEmpty()
        {
            var result = tracker.UpdateContacts(new List<int>());
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void UpdateContacts_MixedNewAndExisting_OnlyReturnsNew()
        {
            tracker.UpdateContacts(new List<int> { 1, 2 });
            var result = tracker.UpdateContacts(new List<int> { 2, 3 });
            Assert.That(result, Is.EquivalentTo(new[] { 3 }));
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            tracker.UpdateContacts(new List<int> { 1 });
            tracker.Reset();
            var result = tracker.UpdateContacts(new List<int> { 1 });
            Assert.That(result, Is.EquivalentTo(new[] { 1 }));
        }
    }
}
