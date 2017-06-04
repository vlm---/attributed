using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace Destructurama.Attributed.Tests
{
    [TestFixture]
    public class AttributedDestructurinPerformaceTests
    {
        [Test]
        public void AttributesImpact()
        {
            BenchmarkRunner.Run<AttributesImpact>();
        }
    }
}
