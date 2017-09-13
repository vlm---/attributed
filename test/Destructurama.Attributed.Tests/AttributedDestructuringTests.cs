using System.Linq;
using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed.Tests
{
    [LogAsScalar]
    public class ImmutableScalar { }

    [LogAsScalar(isMutable: true)]
    public class MutableScalar { }

    public class NotAScalar { }

    public class Customized
    {
        public ImmutableScalar ImmutableScalar { get; set; }
        public MutableScalar MutableScalar { get; set; }
        public NotAScalar NotAScalar { get; set; }

        [NotLogged]
        public string Ignored { get; set; }

        [LoggedOnlyAt(LogEventLevel.Debug)]
        public int IgnoredAboveDebug { get; set; }

        [LoggedOnlyAt(LogEventLevel.Warning)]
        public int IgnoredAboveWarning { get; set; }

        [LoggedOnlyAt(LogEventLevel.Error)]
        public int IgnoredAboveError { get; set; }

        [LoggedOnlyAt(LogEventLevel.Information)]
        [NotLogged]
        public int IgnoredAlways { get; set; }

        [LogAsScalar]
        public NotAScalar ScalarAnyway { get; set; }
    }

    [TestFixture]
    public class AttributedDestructuringTests
    {
        private Customized _testItem;

        [OneTimeSetUp]
        public void PrepareValues()
        {
            _testItem = new Customized
            {
                ImmutableScalar = new ImmutableScalar(),
                MutableScalar = new MutableScalar(),
                NotAScalar = new NotAScalar(),
                Ignored = "Hello, there",
                IgnoredAboveDebug = 1,
                IgnoredAboveWarning = 3,
                IgnoredAboveError = 4,
                IgnoredAlways = -1,
                ScalarAnyway = new NotAScalar()
            };
        }

        [Test]
        public void AttributesAreConsultedWhenDestructuring()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Information("Here is {@Customized}", _testItem);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsFalse(log.IsEnabled(LogEventLevel.Debug));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveDebug"));
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Warning));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveWarning"));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveError"));
            Assert.IsFalse(props.ContainsKey("IgnoredAlways"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());
        }

        [Test]
        public void AttributesAreConsultedWhenDestructuringWithLogLevelSwitch()
        {
            LogEvent evt = null;

            var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);

            var log = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .Destructure.UsingAttributes(levelSwitch)
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            log.Error("Here is {@Customized}", _testItem);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Debug));
            Assert.AreEqual(1, props["IgnoredAboveDebug"].LiteralValue());
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Warning));
            Assert.AreEqual(3, props["IgnoredAboveWarning"].LiteralValue());
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Error));
            Assert.AreEqual(4, props["IgnoredAboveError"].LiteralValue());
            Assert.IsFalse(props.ContainsKey("IgnoredAlways"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());

            levelSwitch.MinimumLevel = LogEventLevel.Information;

            log.Error("Here is {@Customized}", _testItem);

            sv = (StructureValue)evt.Properties["Customized"];
            props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsFalse(log.IsEnabled(LogEventLevel.Debug));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveDebug"));
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Warning));
            Assert.AreEqual(3, props["IgnoredAboveWarning"].LiteralValue());
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Error));
            Assert.AreEqual(4, props["IgnoredAboveError"].LiteralValue());
            Assert.IsFalse(props.ContainsKey("IgnoredAlways"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());

            levelSwitch.MinimumLevel = LogEventLevel.Warning;

            log.Error("Here is {@Customized}", _testItem);

            sv = (StructureValue)evt.Properties["Customized"];
            props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsFalse(log.IsEnabled(LogEventLevel.Debug));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveDebug"));
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Warning));
            Assert.AreEqual(3, props["IgnoredAboveWarning"].LiteralValue());
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Error));
            Assert.AreEqual(4, props["IgnoredAboveError"].LiteralValue());
            Assert.IsFalse(props.ContainsKey("IgnoredAlways"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());

            levelSwitch.MinimumLevel = LogEventLevel.Error;

            log.Error("Here is {@Customized}", _testItem);

            sv = (StructureValue)evt.Properties["Customized"];
            props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
            Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
            Assert.IsFalse(props.ContainsKey("Ignored"));
            Assert.IsFalse(log.IsEnabled(LogEventLevel.Debug));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveDebug"));
            Assert.IsFalse(log.IsEnabled(LogEventLevel.Warning));
            Assert.IsFalse(props.ContainsKey("IgnoredAboveWarning"));
            Assert.IsTrue(log.IsEnabled(LogEventLevel.Error));
            Assert.AreEqual(4, props["IgnoredAboveError"].LiteralValue());
            Assert.IsFalse(props.ContainsKey("IgnoredAlways"));
            Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());
        }
    }
}
