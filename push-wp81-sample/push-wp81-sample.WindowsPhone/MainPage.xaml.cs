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
        private const string VariantUuid = "a01e14f5-7f4f-4bdb-8fee-2d6f92e424fa";
        private const string VariantSecret = "9c5c6d60-902a-46b5-af14-a76538291d57";
        private const string BaseUrl = "http://cfms-push-service-dev.main.vchs.cfms-apps.com";
        private const string EnvironmentUuid = "a6b0ffd6-f944-46b9-89f9-132c5550ba92";
        private const string EnvironmentKey = "647d9c48-5ce5-4196-807c-e8fec679d38d";

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

        private async void TestPushButton_OnClick(object sender, RoutedEventArgs e)
        {

            var httpRequest = WebRequest.CreateHttp(String.Format("{0}/{1}", BaseUrl, "/v1/push"));
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

            string jsonResponse = null;
            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                jsonResponse = await reader.ReadToEndAsync();
                Log("Error requesting push message: " + jsonResponse);
            }
        }

        private string BasicAuthorizationValue(string environmentUuid, string environementKey)
        {
            var stringToEncode = String.Format("{0}:{1}", environmentUuid, environementKey);
            var data = Encoding.UTF8.GetBytes(stringToEncode);
            var base64 = Convert.ToBase64String(data);
            return String.Format("Basic {0}", base64);
        }
    }
}
