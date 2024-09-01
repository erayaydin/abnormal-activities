using Horde.Abnormal.Shared;
using NUnit.Framework;

namespace Horde.Abnormal.Tests.Runtime.Shared
{
    [TestFixture]
    public class FloatExtensionTests
    {
        [Test]
        public void NormalizeWithinRangeReturnsSameValue()
        {
            Assert.AreEqual(0f, 0f.Normalize(), "Normalize should return the same value for angles within the range.");
            Assert.AreEqual(-60f, -60f, "Normalize should return the same value for angles within the range.");
            Assert.AreEqual(60f, 60f, "Normalize should return the same value for angles within the range.");
        }

        [Test]
        public void NormalizeGreaterThan360ReturnsCorrectValue()
        {
            Assert.AreEqual(1f, 361f.Normalize(), "Normalize should return the correct value for angles greater than 360.");
        }

        [Test]
        public void NormalizeNegative360ReturnsCorrectValue()
        {
            Assert.AreEqual(-1f, -361f.Normalize(), "Normalize should return the correct value for angles less than -360.");
        }
        
        [Test]
        public void ClampAngleBelowRangeReturnsMin()
        {
            const float angle = -50f;
            const float min = -30f;
            const float max = 30f;

            var clampedAngle = angle.Clamp(min, max);

            Assert.AreEqual(min, clampedAngle, $"Expected angle to be clamped to the minimum value {min}, but got {clampedAngle}.");
        }

        [Test]
        public void ClampAngleAboveRangeReturnsMax()
        {
            const float angle = 45f;
            const float min = -30f;
            const float max = 30f;

            var clampedAngle = angle.Clamp(min, max);

            Assert.AreEqual(max, clampedAngle, $"Expected angle to be clamped to the maximum value {max}, but got {clampedAngle}.");
        }

        [Test]
        public void ClampAngleWithinRangeReturnsSameAngle()
        {
            const float angle = 15f;
            const float min = -30f;
            const float max = 30f;

            var clampedAngle = angle.Clamp(min, max);

            Assert.AreEqual(angle, clampedAngle, $"Expected angle to remain {angle} as it is within range, but got {clampedAngle}.");
        }

        [Test]
        public void ClampMinAndMaxAreSameReturnsThatValue()
        {
            const float angle = 10f;
            const float min = 5f;
            const float max = 5f;

            var clampedAngle = angle.Clamp(min, max);

            Assert.AreEqual(min, clampedAngle, $"Expected angle to be {min} as min and max are the same, but got {clampedAngle}.");
        }

        [Test]
        public void ClampAngleExactlyAtMinReturnsMin()
        {
            const float min = -30f;
            const float max = 30f;

            var clampedAngle = min.Clamp(min, max);

            Assert.AreEqual(min, clampedAngle, $"Expected angle to be clamped to the minimum value {min}, but got {clampedAngle}.");
        }

        [Test]
        public void ClampAngleExactlyAtMaxReturnsMax()
        {
            const float min = -30f;
            const float max = 30f;

            var clampedAngle = max.Clamp(min, max);

            Assert.AreEqual(max, clampedAngle, $"Expected angle to be clamped to the maximum value {max}, but got {clampedAngle}.");
        }
    }
}