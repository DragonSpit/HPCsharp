using HPCsharp;
using NUnit.Framework;

namespace HPCSharp.UnitTests
{
    [TestFixture]
    public sealed class FillTests
    {
        [Test]
        [TestCase((byte)1)]
        [TestCase((sbyte)1)]
        [TestCase((short)1)]
        [TestCase((ushort)1)]
        [TestCase((int)1)]
        [TestCase((uint)1)]
        [TestCase((long)1)]
        [TestCase((ulong)1)]
        public void ShouldFillUsingBlockCopy<T>(T fillValue) where T : struct
        {
            var array = new T[100];
            array.FillUsingBlockCopy(fillValue);

            Assert.That(array, Has.All.EqualTo(fillValue));
        }
    }
}
