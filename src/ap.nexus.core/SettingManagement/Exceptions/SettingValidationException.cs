using ap.nexus.core.Exceptions;

namespace ap.nexus.core.SettingManagement.Exceptions
{
    public class SettingValidationException : NexusException
    {
        /// <summary>
        /// Gets the name of the setting that failed validation
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// Gets the validation error details
        /// </summary>
        public string ValidationError { get; }

        /// <summary>
        /// Gets the invalid value that was provided (as string representation)
        /// </summary>
        public string InvalidValue { get; }

        public SettingValidationException(
            string settingName,
            string validationError,
            object invalidValue)
            : base(FormatMessage(settingName, validationError))
        {
            SettingName = settingName;
            ValidationError = validationError;
            InvalidValue = invalidValue?.ToString() ?? "null";

            AddData("SettingName", settingName);
            AddData("ValidationError", validationError);
            AddData("InvalidValue", InvalidValue);
        }

        public SettingValidationException(
            string settingName,
            string validationError,
            object invalidValue,
            Exception innerException)
            : base(FormatMessage(settingName, validationError), innerException)
        {
            SettingName = settingName;
            ValidationError = validationError;
            InvalidValue = invalidValue?.ToString() ?? "null";

            AddData("SettingName", settingName);
            AddData("ValidationError", validationError);
            AddData("InvalidValue", InvalidValue);
        }

        private static string FormatMessage(string settingName, string validationError)
        {
            return $"Setting '{settingName}' validation failed: {validationError}";
        }
    }
}
