namespace Destructurama.Attributed.Tests.Tested
{
    public class Customized
    {
        public ImmutableScalar ImmutableScalar { get; set; }
        public MutableScalar MutableScalar { get; set; }
        public NotAScalar NotAScalar { get; set; }

        [NotLogged]
        public string Ignored { get; set; }

        [LogAsScalar]
        public NotAScalar ScalarAnyway { get; set; }
    }

    public class CustomizedClean
    {
        public ImmutableScalarClean ImmutableScalar { get; set; }
        public MutableScalarClean MutableScalar { get; set; }
        public NotAScalar NotAScalar { get; set; }
        public string Ignored { get; set; }
        public NotAScalar ScalarAnyway { get; set; }
    }
}