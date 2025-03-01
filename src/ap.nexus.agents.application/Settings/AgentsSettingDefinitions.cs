﻿using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.Settings.Definitions;

namespace ap.nexus.agents.application.Settings
{
    public class AgentsSettingDefinitions
    {       

        // Constants for setting names - makes it easier to reference settings
        public const string BatchSize = "Nexus:Agents:BatchSize";
        public const string OcrEnabled = "Nexus:Agents:IsOcrEnabled";
        public const string OcrConfig = "Nexus:Agents:GetOcrConfig";
        public const string AllowedFileTypes = "Nexus:Agents:AllowedFileTypes";

        public static IEnumerable<ISettingDefinition> GetDefinitions()
        {
            return new List<ISettingDefinition>
            {
                new NumberSettingDefinition(BatchSize)
                {
                    DisplayName = "Batch Size",
                    Description = "Number of documents to process in each batch",
                    DefaultValue = 100,
                    Minimum = 1,
                    Maximum = 1000
                },
                new StringSettingDefinition(AllowedFileTypes)
                {
                    DisplayName = "Allowed File Types",
                    Description = "Comma-separated list of allowed file extensions",
                    DefaultValue = ".pdf,.doc,.docx",
                    AllowedValues = new[] { ".pdf", ".doc", ".docx", ".txt" }
                },
                new BooleanSettingDefinition(OcrEnabled)
                {
                    DisplayName = "Enable OCR",
                    Description = "Enable Optical Character Recognition for scanned documents",
                    DefaultValue = true
                },
                new JsonSettingDefinition(OcrConfig, typeof(OcrConfig))
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

            };
        }

        
    }

    public class OcrConfig
    {
        public string Language { get; set; }
        public int DPI { get; set; }
        public bool EnablePreprocessing { get; set; }
    }
}
