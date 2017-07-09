using System;
using Serilog.Events;

namespace Destructurama.Attributed
{
    /// <summary>
    /// Specified that a property should be included when destructuring an object only when logging level is higher than desired level.
    /// When logging switch is not provided <see cref="LoggedOnlyAtAttribute"/> behaves same as <see cref="NotLoggedAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LoggedOnlyAtAttribute : Attribute
    {
        public LogEventLevel DesiredLevel { get; }

        public LoggedOnlyAtAttribute(LogEventLevel desiredLevel)
        {
            DesiredLevel = desiredLevel;
        }
    }
}