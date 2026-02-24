using System;
using NUnit.Framework;
using Project.Core.Random;

namespace Project.Tests.Random
{
    public class SeededRandomServiceTests
    {
        [Test]
        public void SameSeed_ValueSequence_IsDeterministic()
        {
            var first = new SeededRandomService(12345);
            var second = new SeededRandomService(12345);

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(first.Value(), second.Value());
            }
        }

        [Test]
        public void Range_InvalidBounds_ThrowsArgumentException()
        {
            var randomService = new SeededRandomService(1);

            Assert.Throws<ArgumentException>(() => randomService.Range(10, 10));
            Assert.Throws<ArgumentException>(() => randomService.Range(10, 9));
        }
    }
}
