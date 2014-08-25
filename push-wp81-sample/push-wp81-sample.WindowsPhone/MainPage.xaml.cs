using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using MSSPush_Base.Models;
using MSSPush_Universal;

namespace push_wp81_sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string VariantUuid = "a01e14f5-7f4f-4bdb-8fee-2d6f92e424fa";
        private const string VariantSecret = "9c5c6d60-902a-46b5-af14-a76538291d57";
        private const string BaseUrl = "http://cfms-push-service-dev.main.vchs.cfms-apps.com";

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Text = "Press 'Register' to try registering for push notifications.";
        }

        private async void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            Log("Registering for push...");
            MSSPush push = MSSPush.SharedInstance;
            MSSParameters parameters = GetMssParameters();
            await push.RegisterForPushAsync(parameters, completionAction: (result) =>
            {
                if (result.Succeeded)
                {
                    Log("Push registration succeeded.");
                }
                else
                {
                    Log("Push registration failed: " + result.ErrorMessage + ".");
                }
            });
        }

        private static MSSParameters GetMssParameters()
        {
            return new MSSParameters(VariantUuid, VariantSecret, BaseUrl);
        }

        private void Log(string logString)
        {
            OutputTextBox.Text = logString + "\n" + OutputTextBox.Text;
            Debug.WriteLine(logString);
        }

        private async void UnregisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            MSSPush push = MSSPush.SharedInstance;
            MSSParameters parameters = GetMssParameters();
            await push.UnregisterForPushAsync(parameters, (result) =>
            {
                if (result.Succeeded)
                {
                    Log("Push unregistration succeeded.");
                }
                else
                {
                    Log("Push unregistration failed: " + result.ErrorMessage + ".");
                }
            });
        }

        private void TestPushButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO - implement me
        }
    }
}
