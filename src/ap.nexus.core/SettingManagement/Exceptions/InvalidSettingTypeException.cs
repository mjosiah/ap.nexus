using ap.nexus.core.Exceptions;

namespace ap.nexus.core.SettingManagement.Exceptions
{
    public class InvalidSettingTypeException : NexusException
    {
        /// <summary>
        /// Gets the name of the setting
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// Gets the expected type of the setting
        /// </summary>
        public Type ExpectedType { get; }

        /// <summary>
        /// Gets the actual type that was provided
        /// </summary>
        public Type ActualType { get; }

        public InvalidSettingTypeException(
            string settingName,
            Type expectedType,
            Type actualType)
            : base(FormatMessage(settingName, expectedType, actualType))
        {
            SettingName = settingName;
            ExpectedType = expectedType;
            ActualType = actualType;

            AddData("SettingName", settingName);
            AddData("ExpectedType", expectedType.FullName);
            AddData("ActualType", actualType.FullName);
        }

        public InvalidSettingTypeException(
            string settingName,
            Type expectedType,
            Type actualType,
            Exception innerException)
            : base(FormatMessage(settingName, expectedType, actualType), innerException)
        {
            SettingName = settingName;
            ExpectedType = expectedType;
            ActualType = actualType;

            AddData("SettingName", settingName);
            AddData("ExpectedType", expectedType.FullName);
            AddData("ActualType", actualType.FullName);
        }

        private static string FormatMessage(string settingName, Type expectedType, Type actualType)
        {
            return $"Invalid type for setting '{settingName}'. Expected '{expectedType.Name}' but got '{actualType.Name}'.";
        }
    }
}
