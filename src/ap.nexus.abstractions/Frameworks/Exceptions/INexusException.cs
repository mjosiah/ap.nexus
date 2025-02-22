namespace ap.nexus.abstractions.Frameworks.Exceptions
{
    /// <summary>
    /// Base contract for all Nexus-specific exceptions
    /// </summary>
    public interface INexusException
    {
        /// <summary>
        /// Gets a human-readable error message
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets additional error details, if any
        /// </summary>
        IDictionary<string, object> Data { get; }
    }
}
