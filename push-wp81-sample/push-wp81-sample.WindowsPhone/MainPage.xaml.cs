using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.PushNotifications;
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
using Newtonsoft.Json;
using push_wp81_sample.Model;

namespace push_wp81_sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string VariantUuid = "420ac641-e9da-41fa-a9bf-d028a747bc37";
        private const string VariantSecret = "e4417af0-3f45-4735-9da5-ed6c371682c5";
        private const string BaseUrl = "http://cfms-push-service-dev.main.vchs.cfms-apps.com";
        private const string EnvironmentUuid = "3f19f4a4-67b4-45a9-aa19-e73b9fc8bc68";
        private const string EnvironmentKey = "92d293de-ebf7-4426-8546-b98c8ebb4333";

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
                    if (result.RawNotificationChannel != null)
                    {
                        result.RawNotificationChannel.PushNotificationReceived += OnPushNotificationReceived;
                    }
                }
                else
                {
                    Log("Push registration failed: " + result.ErrorMessage + ".");
                }
            });
        }

        private void OnPushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            Debug.WriteLine("Notification received!");
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

        private async void TestPushButton_OnClick(object sender, RoutedEventArgs e)
        {

            var httpRequest = WebRequest.CreateHttp(String.Format("{0}/v1/push", BaseUrl));
            httpRequest.Method = "POST";
            httpRequest.Accept = "application/json";
            httpRequest.Headers[HttpRequestHeader.Authorization] = BasicAuthorizationValue(EnvironmentUuid, EnvironmentKey);
           
            httpRequest.ContentType = "application/json; charset=UTF-8";
            using (var stream = await Task.Factory.FromAsync<Stream>(httpRequest.BeginGetRequestStream, httpRequest.EndGetRequestStream, null))
            {
                var settings = new Settings();
                object deviceUuid;
                if (!settings.TryGetValue("PushDeviceUuid", out deviceUuid))
                {
                    Log("This device is not registered for push");
                    return;
                }
                var deviceUuids = new string[] {deviceUuid as String};
                var request = PushRequest.MakePushRequest("This message was pushed at " + System.DateTime.Now, deviceUuids);
                var jsonString = JsonConvert.SerializeObject(request);
                var bytes = Encoding.UTF8.GetBytes(jsonString);
                stream.Write(bytes, 0, bytes.Length);
            }

            WebResponse webResponse;
            try
            {
                webResponse = await Task.Factory.FromAsync<WebResponse>(httpRequest.BeginGetResponse, httpRequest.EndGetResponse, null);
            }
            catch (WebException ex)
            {
                webResponse = ex.Response;
            }

            var httpResponse = webResponse as HttpWebResponse;
            if (httpResponse == null)
            {
                Log("Error requesting push message: Unexpected/invalid response type. Unable to parse JSON.");
                return;
            }

            if (IsSuccessfulHttpStatusCode(httpResponse.StatusCode))
            {
                Log("Server accepted message for delivery.");
                return;
            }

            string jsonResponse = null;
            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                jsonResponse = await reader.ReadToEndAsync();
                Log("Error requesting push message: " + jsonResponse);
            }
        }

        private bool IsSuccessfulHttpStatusCode(HttpStatusCode statusCode)
        {
            return (statusCode >= HttpStatusCode.OK && statusCode < HttpStatusCode.Ambiguous);
        }

        private string BasicAuthorizationValue(string environmentUuid, string environmentKey)
        {
            var stringToEncode = String.Format("{0}:{1}", environmentUuid, environmentKey);
            var data = Encoding.UTF8.GetBytes(stringToEncode);
            var base64 = Convert.ToBase64String(data);
            return String.Format("Basic {0}", base64);
        }
    }
}
