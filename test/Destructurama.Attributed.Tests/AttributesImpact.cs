using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Destructurama.Attributed.Tests.Tested;
using Serilog;

namespace Destructurama.Attributed.Tests
{
    public class AttributesImpact
    {
        public ILogger Logger { get; private set; }
        public ILogger LoggerConcurrent { get; private set; }

        public Customized Customized { get; private set; }
        public CustomizedClean CustomizedClean { get; private set; }

        [Params(1, 3)]
        public int Count { get; set; }

        [Setup]
        public void Setup()
        {
            Customized = new Customized
            {
                ImmutableScalar = new ImmutableScalar(),
                MutableScalar = new MutableScalar(),
                NotAScalar = new NotAScalar(),
                Ignored = "Hello, there",
                ScalarAnyway = new NotAScalar()
            };
            CustomizedClean = new CustomizedClean
            {
                ImmutableScalar = new ImmutableScalarClean(),
                MutableScalar = new MutableScalarClean(),
                NotAScalar = new NotAScalar(),
                Ignored = "Hello, there",
                ScalarAnyway = new NotAScalar()
            };
            Logger = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .CreateLogger();
            LoggerConcurrent = new LoggerConfiguration()
                .Destructure.UsingAttributesConcurrent()
                .CreateLogger();
        }

        [Benchmark]
        public void WithAttributes()
        {
            var tasks = new Task[Count];
            for (int i = 0; i < Count; i++)
            {
                tasks[i] = Task.Run(() => Logger.Information("Here is {@Customized}", Customized));
            }
            Task.WaitAll(tasks);
        }

        [Benchmark(Baseline = true)]
        public void WithoutAttributes()
        {
            var tasks = new Task[Count];
            for (int i = 0; i < Count; i++)
            {
                tasks[i] = Task.Run(() => Logger.Information("Here is {@Customized}", CustomizedClean));
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void WithAttributesConcurrent()
        {
            var tasks = new Task[Count];
            for (int i = 0; i < Count; i++)
            {
                tasks[i] = Task.Run(() => LoggerConcurrent.Information("Here is {@Customized}", Customized));
            }
            Task.WaitAll(tasks);
        }

        [Benchmark]
        public void WithoutAttributesConcurrent()
        {
            var tasks = new Task[Count];
            for (int i = 0; i < Count; i++)
            {
                tasks[i] = Task.Run(() => LoggerConcurrent.Information("Here is {@Customized}", CustomizedClean));
            }
            Task.WaitAll(tasks);
        }
    }
}