using FinancialThrottleService.Application.Logic;
using Xunit;

namespace FinancialThrottleService.Application.Tests.Logic
{
    public class GroupRetryTrackerTests
    {
        private const string GroupKey = "RAS_STAJ107|282|21";

        [Fact]
        public void RecordFailure_CalledNineTimes_DoesNotSuspendGroup()
        {
            var tracker = new GroupRetryTracker();

            bool suspended = false;
            for (int i = 0; i < 9; i++)
                suspended = tracker.RecordFailure(GroupKey, "test hatasi");

            Assert.False(suspended);
            Assert.False(tracker.IsSuspended(GroupKey));
        }

        [Fact]
        public void RecordFailure_CalledTenthTime_SuspendsGroup()
        {
            var tracker = new GroupRetryTracker();

            bool suspended = false;
            for (int i = 0; i < 10; i++)
                suspended = tracker.RecordFailure(GroupKey, "test hatasi");

            Assert.True(suspended);
            Assert.True(tracker.IsSuspended(GroupKey));
        }

        [Fact]
        public void RecordFailure_CalledEleventhTime_DoesNotReturnTrueAgain()
        {
            var tracker = new GroupRetryTracker();
            for (int i = 0; i < 10; i++)
                tracker.RecordFailure(GroupKey);

            bool suspendedAgain = tracker.RecordFailure(GroupKey);

            Assert.False(suspendedAgain);
            Assert.True(tracker.IsSuspended(GroupKey));
        }

        [Fact]
        public void RecordSuccess_ClearsSuspendedGroup()
        {
            var tracker = new GroupRetryTracker();
            for (int i = 0; i < 10; i++)
                tracker.RecordFailure(GroupKey);

            Assert.True(tracker.IsSuspended(GroupKey));

            tracker.RecordSuccess(GroupKey);

            Assert.False(tracker.IsSuspended(GroupKey));
        }

        [Fact]
        public void RecordWait_MarksGroupAsWaiting_ClearWaitRemovesIt()
        {
            var tracker = new GroupRetryTracker();

            tracker.RecordWait(GroupKey, "Required=[1,2,3] but present=[1,2]");

            Assert.True(tracker.IsWaiting(GroupKey));
            Assert.Equal("Required=[1,2,3] but present=[1,2]", tracker.GetWaitReason(GroupKey));

            tracker.ClearWait(GroupKey);

            Assert.False(tracker.IsWaiting(GroupKey));
        }

        [Fact]
        public void MarkForceSend_IsForceSendReturnsTrue_ClearForceSendThenFalse()
        {
            var tracker = new GroupRetryTracker();

            Assert.False(tracker.IsForceSend(GroupKey));

            tracker.MarkForceSend(GroupKey);
            Assert.True(tracker.IsForceSend(GroupKey));

            tracker.ClearForceSend(GroupKey);
            Assert.False(tracker.IsForceSend(GroupKey));
        }

        [Fact]
        public void MarkForceSend_ImmediatelyUnsuspendsGroup()
        {
          .
            var tracker = new GroupRetryTracker();
            for (int i = 0; i < 10; i++)
                tracker.RecordFailure(GroupKey);
            Assert.True(tracker.IsSuspended(GroupKey));

            tracker.MarkForceSend(GroupKey);

            Assert.False(tracker.IsSuspended(GroupKey));
        }

        [Fact]
        public void IsRetryDue_NewlySuspendedGroup_ReturnsFalseImmediately()
        {
            var tracker = new GroupRetryTracker();
            for (int i = 0; i < 10; i++)
                tracker.RecordFailure(GroupKey);

            Assert.False(tracker.IsRetryDue(GroupKey));
        }

        [Fact]
        public void GetSuspendedGroups_ParsesGroupKeyCorrectly()
        {
            var tracker = new GroupRetryTracker();
            for (int i = 0; i < 10; i++)
                tracker.RecordFailure(GroupKey, "baglanti hatasi");

            var suspended = Assert.Single(tracker.GetSuspendedGroups());

            Assert.Equal("RAS_STAJ107", suspended.DatabaseName);
            Assert.Equal(282, suspended.SecurityId);
            Assert.Equal(21, suspended.TemplateId);
            Assert.Equal(10, suspended.FailureCount);
            Assert.Equal("baglanti hatasi", suspended.LastError);
        }
    }
}
