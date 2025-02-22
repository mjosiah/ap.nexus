using ap.nexus.core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ap.nexus.core.SettingManagement.Exceptions
{
    /// <summary>
    /// Exception thrown when a setting value cannot be converted to the requested type
    /// </summary>
    public class SettingValueConversionException : NexusException
    {
        /// <summary>
        /// Gets the name of the setting that failed conversion
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// Gets the target type that the conversion was attempting to convert to
        /// </summary>
        public Type TargetType { get; }

        public SettingValueConversionException(
            string settingName,
            Type targetType,
            Exception innerException)
            : base(FormatMessage(settingName, targetType), innerException)
        {
            SettingName = settingName;
            TargetType = targetType;

            AddData("SettingName", settingName);
            AddData("TargetType", targetType.FullName);

            // If we have conversion details in the inner exception, add them
            if (innerException is JsonException jsonException)
            {
                AddData("JsonPath", jsonException.Path);
                AddData("LineNumber", jsonException.LineNumber);
            }
            else if (innerException is FormatException)
            {
                AddData("ConversionError", "Invalid format for the target type");
            }
            else if (innerException is OverflowException)
            {
                AddData("ConversionError", "Value is outside the range of the target type");
            }
        }

        private static string FormatMessage(string settingName, Type targetType)
        {
            return $"Failed to convert setting '{settingName}' to type '{targetType.Name}'.";
        }
    }
}
