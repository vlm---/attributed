using System.Linq;
using Destructurama.Attributed.Tests.Support;
using Destructurama.Attributed.Tests.Tested;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.Attributed.Tests
{
    [TestFixture]
    public class AttributedDestructuringTests
    {
        [Test]
        public void AttributesAreConsultedWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new Customized
            {
                ImmutableScalar = new ImmutableScalar(),
                MutableScalar = new MutableScalar(),
                NotAScalar = new NotAScalar(),
                Ignored = "Hello, there",
                ScalarAnyway = new NotAScalar()
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());
        }

        [Test]
        public void AttributesAreConsultedWhenDestructuringConcurrent()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributesConcurrent()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new Customized
            {
                ImmutableScalar = new ImmutableScalar(),
                MutableScalar = new MutableScalar(),
                NotAScalar = new NotAScalar(),
                Ignored = "Hello, there",
                ScalarAnyway = new NotAScalar()
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());
        }
    }
}
