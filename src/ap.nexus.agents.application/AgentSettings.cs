using ap.nexus.core.Settings.Definitions;
using ap.nexus.settingmanager.Application;

namespace ap.nexus.agents.application
{
    public class SmartDocsSettings
    {
        private readonly ISettingManager _settingManager;

        public SmartDocsSettings(ISettingManager settingManager)
        {
            _settingManager = settingManager;

            // Register settings definitions
            _settingManager.DefineSettingsAsync(new ISettingDefinition[]
            {
                new NumberSettingDefinition("SmartDocs.Ingestion.BatchSize")
                {
                    DisplayName = "Batch Size",
                    Description = "Number of documents to process in each batch",
                    DefaultValue = 100,
                    Minimum = 1,
                    Maximum = 1000
                },

                new StringSettingDefinition("SmartDocs.Ingestion.FileTypes")
                {
                    DisplayName = "Allowed File Types",
                    Description = "Comma-separated list of allowed file extensions",
                    DefaultValue = ".pdf,.doc,.docx",
                    AllowedValues = new[] { ".pdf", ".doc", ".docx", ".txt" }
                },

                new BooleanSettingDefinition("SmartDocs.Processing.EnableOCR")
                {
                    DisplayName = "Enable OCR",
                    Description = "Enable Optical Character Recognition for scanned documents",
                    DefaultValue = true
                },

                new JsonSettingDefinition("SmartDocs.Processing.OCRConfig", typeof(OcrConfig))
                {
                    DisplayName = "OCR Configuration",
                    Description = "Advanced OCR processing settings",
                    DefaultValue = new OcrConfig
                    {
                        Language = "en",
                        DPI = 300,
                        EnablePreprocessing = true
                    }
                }
            });
        }

        // Methods to access settings
        public async Task<int> GetBatchSizeAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<int>(
                "SmartDocs.Ingestion.BatchSize",
                tenantId);
        }

        public async Task<bool> IsOcrEnabledAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<bool>(
                "SmartDocs.Processing.EnableOCR",
                tenantId);
        }

        public async Task<OcrConfig> GetOcrConfigAsync(Guid? tenantId = null)
        {
            return await _settingManager.GetSettingValueAsync<OcrConfig>(
                "SmartDocs.Processing.OCRConfig",
                tenantId);
        }
    }

    public class OcrConfig
    {
        public string Language { get; set; }
        public int DPI { get; set; }
        public bool EnablePreprocessing { get; set; }
    }
}
