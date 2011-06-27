using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Phone.Controls;

using Microsoft.Health.Mobile;
using Microsoft.Phone.Tasks;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO;

namespace MoodTracker
{

    /// <summary>
    /// This is the main page which displays to the user if 
    /// they go through the HealthVault authentication and authorization.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        public const string SettingsFilename = "Settings.xml";
        bool _addingRecord;
        List<string> _currentThingIds = new List<string>();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(SettingsFilename);
            App.HealthVaultService.BeginAuthenticationCheck(AuthenticationCompleted, 
                DoShellAuthentication);
            SetProgressBarVisibility(true);
        }

        void DoShellAuthentication(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            App.HealthVaultService.SaveSettings(SettingsFilename);

            string url;

            if (_addingRecord)
            {
                url = App.HealthVaultService.GetUserAuthorizationUrl();
            }
            else
            {
                url = App.HealthVaultService.GetApplicationCreationUrl();
            }

            App.HealthVaultShellUrl = url;

            // If we are  using hosted browser via the hosted browser page
            Uri pageUri = new Uri("/HostedBrowser.xaml", UriKind.RelativeOrAbsolute);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(pageUri);
            });

        }

        void AuthenticationCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e != null && e.ErrorText != null)
            {
                SetRecordName(e.ErrorText);
                return;
            }

            if (App.HealthVaultService.CurrentRecord == null)
            {
                App.HealthVaultService.CurrentRecord = App.HealthVaultService.Records[0];
            }

            App.HealthVaultService.SaveSettings(SettingsFilename);
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                HealthVaultMethods.GetThings(EmotionalStateModel.TypeId, GetThingsCompleted);
                SetProgressBarVisibility(true);
            }
        }

        void SetRecordName(string recordName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_RecordName.Text = recordName;
            });
        }

        void SetProgressBarVisibility(bool visible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        void SetErrorMesasge(string message)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    ErrorMessage.Text = message;
                    ErrorMessage.Visibility = Visibility.Visible;
                });
        }

        void SetUserToast(string message)
        {
            SetErrorMesasge(message);
        }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);

            if (e.ErrorText == null)
            {

                XElement responseNode = XElement.Parse(e.ResponseXml);
                // using linq to get the latest reading of emotional state
                XElement latestEmotion = (from thingNode in responseNode.Descendants("thing")
                                          orderby Convert.ToDateTime(thingNode.Element("eff-date").Value) descending
                                          select thingNode).FirstOrDefault<XElement>();

                EmotionalStateModel emotionalState =
                    new EmotionalStateModel();
                emotionalState.Parse(latestEmotion.Descendants("data-xml").Descendants("emotion").Single());

                string lastTime = Convert.ToDateTime(latestEmotion.Element("eff-date").Value).ToString();
                   
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    c_LastUpdated.Text += lastTime;
                    c_Mood.Text += System.Enum.GetName(typeof(Mood), emotionalState.Mood);
                    c_Stress.Text += System.Enum.GetName(typeof(Stress), emotionalState.Stress);
                    c_Wellbeing.Text += System.Enum.GetName(typeof(Wellbeing), emotionalState.Wellbeing);
                });
            }
        }

        // Save the reading to HealthVault
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            EmotionalStateModel model = new EmotionalStateModel();
            model.Mood = (Mood)c_MoodSlider.Value;
            model.Stress = (Stress)c_StressSlider.Value;
            model.Wellbeing = (Wellbeing)c_WellbeingSlider.Value;
            model.When = DateTime.Now;
            HealthVaultMethods.PutThings(model, PutThingsCompleted);
            SetProgressBarVisibility(true);
        }

        void PutThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            SetProgressBarVisibility(false);
            if (e.ErrorText != null)
            {
                SetErrorMesasge(e.ErrorText);
            }
            else
            {
                SetUserToast("Mood successfully saved!");
            }
        }

        public string GetSliderValue(Type t, Slider slider)
        {
            return System.Enum.GetName(
                t, (int)slider.Value);
        }

        private void c_MoodSlider_ValueChanged(object sender, 
			System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    MoodSliderValue.Text = GetSliderValue(typeof(Mood), c_MoodSlider);
                });
        }

        private void c_WellbeingSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                WellbeingSliderValue.Text = GetSliderValue(typeof(Wellbeing), 
                    c_WellbeingSlider);
            });
        }

        private void c_StressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                StressSliderValue.Text = GetSliderValue(typeof(Stress),
                    c_StressSlider);
            });
        }
    }
}