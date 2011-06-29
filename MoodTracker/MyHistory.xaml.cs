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
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;

using Microsoft.Health.Mobile;
using System.Xml.Linq;

namespace MoodTracker
{
    public partial class MyHistory : PhoneApplicationPage
    {
        public const string SettingsFilename = "Settings.xml";
        private ObservableCollection<EmotionalStateModel> _emotions =
            new ObservableCollection<EmotionalStateModel>();
        public ObservableCollection<EmotionalStateModel> EmotionList
        {
            get
            {
                if (_emotions == null)
                    _emotions = new ObservableCollection<EmotionalStateModel>();
                return _emotions;
            }
        }
      

        public MyHistory()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.HealthVaultService.LoadSettings(SettingsFilename);
            if (App.HealthVaultService.CurrentRecord != null)
            {
                SetRecordName(App.HealthVaultService.CurrentRecord.RecordName);
                // Get the last emotional state info and try to plot a graph 
                HealthVaultMethods.GetThings(EmotionalStateModel.TypeId, 5, GetThingsCompleted);
                SetProgressBarVisibility(true);
            }
            this.DataContext = this;
        }

        void GetThingsCompleted(object sender, HealthVaultResponseEventArgs e)
        {
            ObservableCollection<EmotionalStateModel> emotionList 
                = new ObservableCollection<EmotionalStateModel>();
            SetProgressBarVisibility(false);
            if (e.ErrorText == null)
            {
                XElement responseNode = XElement.Parse(e.ResponseXml);
                foreach (XElement thing in responseNode.Descendants("thing"))
                {
                    DateTime when = Convert.ToDateTime(thing.Element("eff-date").Value);
                    EmotionalStateModel emotionalState = new EmotionalStateModel();
                    emotionalState.When = when;
                    emotionalState.Parse(thing.Descendants("data-xml").Descendants("emotion").Single());

                    Dispatcher.BeginInvoke( () => {
                        DoItemAdd(
                        emotionalState.When,
                        emotionalState.Mood,
                        emotionalState.Stress,
                        emotionalState.Wellbeing);
                    });
                }
            }
        }

        private void DoItemAdd(DateTime when, Mood mood, 
            Stress stress, Wellbeing wellbeing)
        {
            EmotionalStateModel emotion = new EmotionalStateModel();
            emotion.When = when;
            emotion.Mood = mood;
            emotion.Stress = stress;
            emotion.Wellbeing = wellbeing;
            this.EmotionList.Add(emotion);
        }

        void SetProgressBarVisibility(bool visible)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_progressBar.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        void SetRecordName(string recordName)
        {
            Dispatcher.BeginInvoke(() =>
            {
                c_RecordName.Text = recordName;
            });
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            Uri pageUri = new Uri("/MyMood.xaml", UriKind.RelativeOrAbsolute);
            NavigationService.Navigate(pageUri);
        }

        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {

        }
    }
}