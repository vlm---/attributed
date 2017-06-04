using System;
using Serilog.Events;

namespace Destructurama.Attributed
{
    /// <summary>
    /// Specified that a property should not be included when destructuring an object for logging and logging level is above desired level.
    /// When logging switch is not provided <see cref="NotLoggedAboveAttribute"/> behaves same as <see cref="NotLoggedAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotLoggedAboveAttribute : Attribute
    {
        public LogEventLevel DesiredLevel { get; }

        public NotLoggedAboveAttribute(LogEventLevel desiredLevel)
        {
            DesiredLevel = desiredLevel;
        }
    }
}