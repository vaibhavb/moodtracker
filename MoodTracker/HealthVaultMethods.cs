using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.Health.Mobile;
using System.Xml.Linq;

namespace MoodTracker
{
    public class HealthVaultMethods
    {
        /// <summary>
        /// Get things Method
        /// </summary>
        /// <param name="typeId">HealthVault Type Id</param>
        /// <param name="responseCallback">Response Handler for the Type</param>
        public static void GetThings(string typeId, 
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            string thingXml = @"
            <info>
                <group>
                    <filter>
                        <type-id>{0}</type-id>
                        <thing-state>Active</thing-state>
                    </filter>
                    <format>
                        <section>core</section>
                        <xml/>
                        <type-version-format>{0}</type-version-format>
                    </format>
                </group>
            </info>";

            XElement info = XElement.Parse(string.Format(thingXml, typeId));
            HealthVaultRequest request = new HealthVaultRequest("GetThings", "3", info, responseCallback);
            App.HealthVaultService.BeginSendRequest(request);
        }


        /// <summary>
        /// PutThings Method
        /// </summary>
        /// <param name="typeId">This should be a filter</param>
        /// <param name="responseCallback">Function to resolve callback</param>
        public static void PutThings(HealthRecordItemModel item,
            EventHandler<HealthVaultResponseEventArgs> responseCallback)
        {
            XElement info = XElement.Parse(item.GetXml());
            HealthVaultRequest request = new HealthVaultRequest("PutThings", "2", info, responseCallback);
            App.HealthVaultService.BeginSendRequest(request);
        }
    }
}
