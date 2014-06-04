using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;

namespace AroundMe
{
    public partial class SearchResults : PhoneApplicationPage
    {
        private double latitude;
        private double longitude;
        private string topic;
        private double radius;

        // Api Key (dostałem z Flickr.com
        private const string flickrApiKey = "b91a822cef0fe7a2cdf579a32a5c148f";
        public SearchResults()
        {
            InitializeComponent();
            Loaded += SearchResults_Loaded;
        }

        protected async void SearchResults_Loaded(object sender, RoutedEventArgs e)
        {

            Overlay.Visibility = Visibility.Visible;
            OverlayProgressbar.IsIndeterminate = true;

           // LocationTextBlock.Text = string.Format("location: {0} & {1}", latitude, longitude);
            var images = await FlickrImage.GetFlickrImages(flickrApiKey, 
                topic,
                latitude,
                longitude,
                radius);

            DataContext = images;

            if (!images.Any())
            {
                NoPhotosFoud.Visibility = Visibility.Visible;
            }
            else
            {
                NoPhotosFoud.Visibility = Visibility.Collapsed;
            }

            Overlay.Visibility = Visibility.Collapsed;
            OverlayProgressbar.IsIndeterminate = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            latitude = Convert.ToDouble(NavigationContext.QueryString["latitude"]);
            longitude = Convert.ToDouble(NavigationContext.QueryString["longitude"]);
            topic = NavigationContext.QueryString["topic"];
            radius = Convert.ToDouble(NavigationContext.QueryString["radius"]);
        }

        private void PhotosForLockscreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
            {
                return;
            }

            Storyboard s = new Storyboard();

            DoubleAnimation doubleAni = new DoubleAnimation();

            doubleAni.To = 1;
            doubleAni.Duration = new Duration(TimeSpan.FromMilliseconds(500));

            Storyboard.SetTarget(doubleAni, img);
            Storyboard.SetTargetProperty(doubleAni, new PropertyPath(OpacityProperty));

            s.Children.Add(doubleAni);
            s.Begin();
        }
    }
}