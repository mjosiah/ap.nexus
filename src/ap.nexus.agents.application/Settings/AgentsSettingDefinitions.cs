using ap.nexus.abstractions.Frameworks.SettingManagement;
using ap.nexus.core.Settings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ap.nexus.agents.application.Settings
{
    public class AgentsSettingDefinitions
    {
        // Constants for setting names - makes it easier to reference settings
        public const string MaxProductPrice = "Nexus:Agents:MaxProductPrice";
        public const string MinProductPrice = "Products:MinProductPrice";
        public const string DefaultCurrency = "Products:DefaultCurrency";

        public static IEnumerable<ISettingDefinition> GetDefinitions()
        {
            return new List<ISettingDefinition>
            {
                new SettingDefinition{
                DefaultValue = "1000",
                Description = "Maximum allowed price for a product",
                DisplayName = "Max Product Price",
                Name = MaxProductPrice,
                },
                //    MaxProductPrice, "1000", isVisibleToClients: true),
                //new SettingDefinition(MinProductPrice, "0", isVisibleToClients: true),
                //new SettingDefinition(DefaultCurrency, "USD", isVisibleToClients: true)
            };
        }
    }
}
