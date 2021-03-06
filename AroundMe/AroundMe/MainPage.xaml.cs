﻿using System;
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

            BuildLocalizedApplicationBar();
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();

            UpdateMap();
        }

        private static void SetProggressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }
        private async void UpdateMap()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            SetProggressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Getting Gps Location";

            try
            {
                Geoposition position =
                              await geolocator.GetGeopositionAsync(
                               TimeSpan.FromMinutes(1),
                               TimeSpan.FromSeconds(30));

                SystemTray.ProgressIndicator.Text = "Acquired";
                var gpsCoorCenter = new GeoCoordinate(
                    position.Coordinate.Latitude,
                    position.Coordinate.Longitude);

                AroundMeMap.Center = gpsCoorCenter;
                AroundMeMap.ZoomLevel = 15;
                SetProggressIndicator(false);
            }
            catch(UnauthorizedAccessException)
            {
                MessageBox.Show("Location is disabled in phone settings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           

            
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton =
                new ApplicationBarIconButton(new Uri("/Assets/feature.search.png", UriKind.Relative));
            appBarButton.Text = "Search";
            appBarButton.Click += SearchClick;
            ApplicationBar.Buttons.Add(appBarButton);

            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private void SearchClick(object sender, EventArgs e)
        {
            string topic = HttpUtility.UrlEncode(SearchTopic.Text);
            string navTo = string.Format("/SearchResult.xaml?latitude={0}&longitude={1}&topic={2}&radius={3}",
                AroundMeMap.Center.Latitude,
                AroundMeMap.Center.Longitude,
                topic,
                20);
            NavigationService.Navigate(new Uri(navTo, UriKind.RelativeOrAbsolute));
        }
    }    
}