using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AroundMe.Resources;
using Windows.Devices.Geolocation;
using System.Device.Location;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;

namespace AroundMe
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap();
        }

        private async void UpdateMap()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            Geoposition position = 
               await geolocator.GetGeopositionAsync(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(30));

            var gpsCoorCenter = new GeoCoordinate(
                position.Coordinate.Latitude, 
                position.Coordinate.Longitude);

            // AroundMeMap.SetView(gpsCoorCenter,17);

            AroundMeMap.Center = gpsCoorCenter;
            AroundMeMap.ZoomLevel = 15;

            ResultTextBlock.Text = string.Format("{0} - {1}",
                AroundMeMap.Center.Latitude,
                AroundMeMap.Center.Longitude);

            HttpClient client = new HttpClient();

            // licenses
            // https://www.flickr.com/services/api/flickr.photos.licenses.getInfo.html
            // <license id="4" name="Attribution License" url="http://creativecommons.org/licenses/by/2.0/" />
            // <license id="5" name="Attribution-ShareAlike License" url="http://creativecommons.org/licenses/by-sa/2.0/" />
            // <license id="6" name="Attribution-NoDerivs License" url="http://creativecommons.org/licenses/by-nd/2.0/" />
            // <license id="7" name="No known copyright restrictions" url="http://flickr.com/commons/usage/" />

            string[] licenses = { "4", "5", "6", "7" };
            string license = String.Join(",", licenses);
            license.Replace(",","%2C");

            // Api Key
            string flickrApiKey = "b91a822cef0fe7a2cdf579a32a5c148f";
            // https://api.flickr.com/services/rest/?method=flickr.photos.search&api_key=26d2c903cda4188b061adaeae2eeb16c&text=observatory&lat=41.8988&lon=-87.6123&radius=10&format=json&nojsoncallback=1&api_sig=1f5f200e085ada3e59f9fe4c3061e3e1

            var lata = gpsCoorCenter.Latitude.ToString();
            var lat = lata.Replace(",", ".");
            lat.Replace(",", ".");
            var lona = gpsCoorCenter.Longitude.ToString();
            var lon = lona.Replace(",", ".");
            string url = "https://api.flickr.com/services/rest/" +
            "?method=flickr.photos.search" +
            "&license={0}" +
            "&api_key={1}" +
            "&lat=" + lat +
            "&lon=" + lon +
            "&radius=20" +
            "&format=json" +
            "&nojsoncallback=1";

            //Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            //System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            
            var baseUrl = string.Format(url,
                license,
                flickrApiKey);

          string flickrResult = await client.GetStringAsync(baseUrl);

            //ResultTextBlock.Text = flickrResult;

            // jeśli się udało
          FlickrData apiData = JsonConvert.DeserializeObject<FlickrData>(flickrResult);

          if (apiData.stat == "ok")
          {
              foreach (Photo data in apiData.photos.photo)
              {
                  // forma dla foto
                  // http://farm{farmid}.staticflickr.com/{server-id}/{id}-{secret}{size}.jpg
                  string photoUrl = "http://farm{0}.staticflickr.com/{1}/{2}_{3}_n.jpg";

                  string baseFlickUrl = string.Format(photoUrl,
                      data.farm,
                      data.server,
                      data.id,
                      data.secret);
                  // flickr image control
                  FlickrImage.Source = new BitmapImage(new Uri(baseFlickUrl));

                  break;

              }
          }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }

    public class Photo
    {
        public string id { get; set; }
        public string owner { get; set; }
        public string secret { get; set; }
        public string server { get; set; }
        public int farm { get; set; }
        public string title { get; set; }
        public int ispublic { get; set; }
        public int isfriend { get; set; }
        public int isfamily { get; set; }
    }

    public class Photos
    {
        public int page { get; set; }
        public int pages { get; set; }
        public int perpage { get; set; }
        public string total { get; set; }
        public List<Photo> photo { get; set; }
    }

    public class FlickrData
    {
        public Photos photos { get; set; }
        public string stat { get; set; }
    }
}