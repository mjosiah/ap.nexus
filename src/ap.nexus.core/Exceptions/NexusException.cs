using ap.nexus.abstractions.Frameworks.Exceptions;

namespace ap.nexus.core.Exceptions
{
    public abstract class NexusException : Exception, INexusException
    {
        /// <summary>
        /// Gets additional error details
        /// </summary>
        public new IDictionary<string, object> Data { get; }

        protected NexusException(string message)
            : base(message)
        {
            Data = new Dictionary<string, object>();
        }

        protected NexusException(string message, Exception innerException)
            : base(message, innerException)
        {
            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds additional error detail
        /// </summary>
        /// <param name="key">The detail key</param>
        /// <param name="value">The detail value</param>
        protected void AddData(string key, object value)
        {
            Data[key] = value;
        }
    }
}
