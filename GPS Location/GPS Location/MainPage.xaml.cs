using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpMethod = Windows.Web.Http.HttpMethod;
using HttpRequestMessage = Windows.Web.Http.HttpRequestMessage;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

namespace GPS_Location
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Geolocator geolocator = null;
        bool trackingStatus = false;
        private object _httpClient;

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

        async Task<MapLocationFinderResult> GetAddressFromCoordinatesAsync(Geoposition geoposition)
        {
            var mapLocationFinderResult = await MapLocationFinder.FindLocationsAtAsync(geoposition.Coordinate.Point);
            if (mapLocationFinderResult.Status == MapLocationFinderStatus.Success)
            {
                // hard-coding to only view the first returned possible address ([0]). You might get more than one result, in which case check them against your requirements.
                return mapLocationFinderResult;
                //.Locations[0].Address.Region + " " +
                //  mapLocationFinderResult.Locations[0].Address.Country;
            }

            return null;
        }

        private double _latitude;
        private double _longitude;
        private string _region;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            geolocation.Text = "...Loading Location";
            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                     maximumAge: TimeSpan.FromMinutes(5),
                     timeout: TimeSpan.FromSeconds(10)
                    );

             
                //System.Diagnostics.Debug.WriteLine(geoposition.CivicAddress.City.FirstOrDefault());
                //With this 2 lines of code, the app is able to write on a Text Label the Latitude and the Longitude, given by {{Icode|geoposition}}

                MapLocationFinderResult address = await GetAddressFromCoordinatesAsync(geoposition);
                _latitude = geoposition.Coordinate.Latitude;
                _longitude = geoposition.Coordinate.Longitude;
                _region = address.Locations[0].Address.Town;
                geolocation.Text = "GPS:" + _latitude.ToString("0.00") + ", " + _longitude.ToString("0.00");
                region.Text = "REGION:" + _region;
                country.Text = "COUNTRY:" + address.Locations[0].Address.Country;

                if (region.Text != "")
                {
                       btnSend.Visibility= Visibility.Visible;
                }
             
            }

            //If an error is catch 2 are the main causes: the first is that you forgot to include ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                //exception
            }
        }

        private async void Button_Click1(object sender, RoutedEventArgs e)
        {

            string apiUrl = "http://mobiledatawebapi.azurewebsites.net/api/MobileData";
            Uri apiUrl2 = new Uri("http://mobiledatawebapi.azurewebsites.net/api/MobileData"); 
           
          
            var locationInfo = new LocationInfo()
            {
                Id = Guid.NewGuid(),
                Latitude = _latitude,
                Longitude = _longitude,
                County = _region

            };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(locationInfo);
            HttpClient httpClient = new HttpClient();

            HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod("POST"), new Uri(apiUrl));
            msg.Content = new HttpStringContent(json);
            msg.Content.Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");


            HttpResponseMessage response1 = await httpClient.GetAsync(apiUrl2).AsTask();
            var content = response1.Content;

            var t = content.ReadAsStringAsync().GetResults();
            if (locationInfo.County != null)
            {
                if (content != null)
                {
                    if (content.ReadAsStringAsync().GetResults().Contains(locationInfo.County))
                    {
                        statustxt.Text = "County Already Set";
                    }
                    else
                    {
                        HttpResponseMessage response = await httpClient.SendRequestAsync(msg).AsTask();
                        btnSend.Visibility = Visibility.Collapsed;
                        statustxt.Text = "Sent Successfully!";
                    }
                }

            }
        }
    }
}
