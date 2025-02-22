using ap.nexus.core.Exceptions;

namespace ap.nexus.core.SettingManagement.Exceptions
{
    public class SettingNotFoundException : NexusException
    {
        /// <summary>
        /// Gets the name of the setting that was not found
        /// </summary>
        public string SettingName { get; }

        public SettingNotFoundException(string settingName)
            : base($"Setting '{settingName}' is not defined.")
        {
            SettingName = settingName;
            AddData("SettingName", settingName);
        }

        public SettingNotFoundException(string settingName, Exception innerException)
            : base($"Setting '{settingName}' is not defined.", innerException)
        {
            SettingName = settingName;
            AddData("SettingName", settingName);
        }
    }
}
