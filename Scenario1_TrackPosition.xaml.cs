//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Threading.Tasks;
using Windows.Storage;
#if WINDOWS_UWP
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using System;
#endif

namespace SDKTemplate
{
    public sealed partial class Scenario1_TrackPosition : Page
    {
        // Proides access to location data
        private Geolocator _geolocator = null;

        // A pointer to the main page
        private MainPage _rootPage = MainPage.Current;

        //have the boundry points hard-coded for now
        //get the data from google map
        //C3: 43.52136126029754, -112.05252436575752
        //CAES: 43.521193063384224, -112.05262200476628      
        /*double lon1 = 43.521193063384224;
          double lon2 = 43.52136126029754;
          double lat1 = -112.05262200476628;      
          double lat2 = -112.05252436575752;*/

        //get position data from hololens
        //point 1
        double lat1 = 43.5217697123735;  //max lat
        double lon1 = -112.051320727697;

        //point 2
        double lat2 = 43.521223;
        double lon2 = -112.052643;

        //point 3
        double lat3 = 43.521736;
        double lon3 = -112.051239;

        //point 4
        double lat4 = 43.518898;
        double lon4 = -112.056389;

        //point 5
        double lat5 = 43.519184333333; //min lat
        double lon5 = -112.05100266667;

        //point 6
        double lat6 = 43.5213142047122;
        double lon6 = -112.051653316044;

        //point 7
        double lat7 = 43.5217697123735;
        double lon7 = -112.051320727697;

        //test in my apartment    
        /*double lat1 = 40.521193063384224;
        double lat2 = 53.52136126029754;
        double lon1 = -122.05262200476628;
        double lon2 = -100.05252436575752;*/

        PointsPoly[] pts = new PointsPoly[] { new PointsPoly { lat = 43.5217697123735, lon = -112.051320727697 },
                                              new PointsPoly { lat = 43.5216049021675, lon = -112.053933561353 },
                                              new PointsPoly { lat = 43.521736, lon = -112.051239 },
                                              new PointsPoly { lat = 43.519184333333, lon = -112.05100266667 } };
        public class PointsPoly
        {
            public double lat;
            public double lon;
        }

        public Scenario1_TrackPosition()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (_geolocator != null)
            {
                _geolocator.PositionChanged -= OnPositionChanged;
                _geolocator.StatusChanged -= OnStatusChanged;
            }
        }

        /// <summary>
        /// This is the click handler for the 'StartTracking' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartTracking(object sender, RoutedEventArgs e)
        {
            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // You should set MovementThreshold for distance-based tracking
                    // or ReportInterval for periodic-based tracking before adding event
                    // handlers. If none is set, a ReportInterval of 1 second is used
                    // as a default and a position will be returned every 1 second.
                    //
                    // Value of 2000 milliseconds (2 seconds)
                    // isn't a requirement, it is just an example.
                    _geolocator = new Geolocator { ReportInterval = 1000 };

                    // Subscribe to PositionChanged event to get updated tracking positions
                    _geolocator.PositionChanged += OnPositionChanged;

                    // Subscribe to StatusChanged event to get updates of location status changes
                    _geolocator.StatusChanged += OnStatusChanged;

                    _rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    StartTrackingButton.IsEnabled = false;
                    StopTrackingButton.IsEnabled = true;
                    break;

                case GeolocationAccessStatus.Denied:
                    _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);
                    LocationDisabledMessage.Visibility = Visibility.Visible;
                    break;

                case GeolocationAccessStatus.Unspecified:
                    _rootPage.NotifyUser("Unspecificed error!", NotifyType.ErrorMessage);
                    LocationDisabledMessage.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// This is the click handler for the 'StopTracking' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopTracking(object sender, RoutedEventArgs e)
        {
            _geolocator.PositionChanged -= OnPositionChanged;
            _geolocator.StatusChanged -= OnStatusChanged;
            _geolocator = null;

            StartTrackingButton.IsEnabled = true;
            StopTrackingButton.IsEnabled = false;

            // Clear status
            _rootPage.NotifyUser("", NotifyType.StatusMessage);
        }

        /// <summary>
        /// Event handler for PositionChanged events. It is raised when
        /// a location is available for the tracking session specified.
        /// </summary>
        /// <param name="sender">Geolocator instance</param>
        /// <param name="e">Position data</param>
        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _rootPage.NotifyUser("Location updated.", NotifyType.StatusMessage);
                UpdateLocationDataAsync(e.Position);
            });
        }

        /// <summary>
        /// Event handler for StatusChanged events. It is raised when the
        /// location status in the system changes.
        /// </summary>
        /// <param name="sender">Geolocator instance</param>
        /// <param name="e">Statu data</param>
        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Show the location setting message only if status is disabled.
                LocationDisabledMessage.Visibility = Visibility.Collapsed;

                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        // Location platform is providing valid data.
                        ScenarioOutput_Status.Text = "Ready";
                        _rootPage.NotifyUser("Location platform is ready.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix.
                        ScenarioOutput_Status.Text = "Initializing";
                        _rootPage.NotifyUser("Location platform is attempting to obtain a position.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        ScenarioOutput_Status.Text = "No data";
                        _rootPage.NotifyUser("Not able to determine the location.", NotifyType.ErrorMessage);
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        ScenarioOutput_Status.Text = "Disabled";
                        _rootPage.NotifyUser("Access to location is denied.", NotifyType.ErrorMessage);

                        // Show message to the user to go to location settings
                        LocationDisabledMessage.Visibility = Visibility.Visible;

                        // Clear cached location data if any
                        UpdateLocationDataAsync(null);
                        break;

                    case PositionStatus.NotInitialized:
                        // The location platform is not initialized. This indicates that the application
                        // has not made a request for location data.
                        ScenarioOutput_Status.Text = "Not initialized";
                        _rootPage.NotifyUser("No request for location is made yet.", NotifyType.StatusMessage);
                        break;

                    case PositionStatus.NotAvailable:
                        // The location platform is not available on this version of the OS.
                        ScenarioOutput_Status.Text = "Not available";
                        _rootPage.NotifyUser("Location is not available on this version of the OS.", NotifyType.ErrorMessage);
                        break;

                    default:
                        ScenarioOutput_Status.Text = "Unknown";
                        _rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                        break;
                }
            });
        }

        /// <summary>
        /// Updates the user interface with the Geoposition data provided
        /// </summary>
        /// <param name="position">Geoposition to display its details</param>
        private async Task UpdateLocationDataAsync(Geoposition position)
        {
            if (position == null)
            {
                ScenarioOutput_Latitude.Text = "No data";
                ScenarioOutput_Longitude.Text = "No data";
                ScenarioOutput_Accuracy.Text = "No data";
                ScenarioOutput_IsRemoteSource.Text = "No data";
            }
            else
            {
                ScenarioOutput_Latitude.Text = position.Coordinate.Point.Position.Latitude.ToString();
                ScenarioOutput_Longitude.Text = position.Coordinate.Point.Position.Longitude.ToString();
                ScenarioOutput_Accuracy.Text = position.Coordinate.Accuracy.ToString();
                //ScenarioOutput_IsRemoteSource.Text = position.Coordinate.IsRemoteSource.ToString();

                //write the location data to a text file
                //LocationData2FileAsync(ScenarioOutput_Latitude.Text, ScenarioOutput_Longitude.Text);
                LocationData2File(ScenarioOutput_Latitude.Text, ScenarioOutput_Longitude.Text);
                //CreateMyFileAsync();
                await CreateFileButton_Click();
                //if (IsInsideSquare(position,lon6,lon7,lat6,lat7))  //test using a square
                /*if (IsPointInPolygon(pts, position))                 //test using a polygon
                {
                    ScenarioOutput_IsRemoteSource.Text = "yes";
                }
                else
                {
                    ScenarioOutput_IsRemoteSource.Text = "no";
                }*/

                //test using a line
                if (IsLeft(pts[0], pts[3], position))
                {
                    ScenarioOutput_IsRemoteSource.Text = "yes";
                }
                else
                {
                    ScenarioOutput_IsRemoteSource.Text = "no";
                }
            }
        }

        private bool IsInsideSquare(Geoposition position, double lon1, double lon2, double lat1, double lat2)
        {

            double currentLat = position.Coordinate.Point.Position.Latitude;
            double currentLon = position.Coordinate.Point.Position.Longitude;

            if (lon1 < currentLon && lon2 > currentLon && lat1 < currentLat && lat2 > currentLat)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsPointInPolygon(PointsPoly[] polygon, Geoposition position)
        {

            double currentLat = position.Coordinate.Point.Position.Latitude;
            double currentLon = position.Coordinate.Point.Position.Longitude;

            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].lon < currentLon && polygon[j].lon >= currentLon || polygon[j].lon < currentLon && polygon[i].lon >= currentLon)
                {
                    if (polygon[i].lat + (currentLon - polygon[i].lon) / (polygon[j].lon - polygon[i].lon) * (polygon[j].lat - polygon[i].lat) < currentLat)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
        public bool IsLeft(PointsPoly a, PointsPoly b, Geoposition position)
        {

            double currentLat = position.Coordinate.Point.Position.Latitude;
            double currentLon = position.Coordinate.Point.Position.Longitude;

            return ((b.lat - a.lat) * (currentLon - a.lon) - (b.lon - a.lon) * (currentLat - a.lat)) > 0;
        }

        public async Task LocationData2FileAsync(string lat, string lon)
        {
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync("sample.txt",
                Windows.Storage.CreationCollisionOption.ReplaceExisting);

            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, lat);
        }
        public void LocationData2File(string lat, string lon)
        {
            //string filePath = @"Documents\location.txt";
            string filePath = @"U:\USERS\inlin\Documents\location.txt";
            //string filePath = Path.Combine(Application.persistentDataPath, "MyFile.txt");
            List<string> lines = new List<string>();
            lines.Add(lat);
            lines.Add(lon);
            File.WriteAllLines(filePath, lines);
        }
        public async Task CreateMyFileAsync()
        {
            //Get local folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            //Create file
            StorageFile textFileForWrite = await storageFolder.CreateFileAsync("LocalText.txt");

            //Write to file
            await FileIO.WriteTextAsync(textFileForWrite, "Text written to file from code");
        }

        public StorageFile sampleFile = null;
        public const string filename = "loc.txt";
        public async Task CreateFileButton_Click()
        {
            StorageFolder storageFolder = await KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.CameraRoll);
            try
            {
                sampleFile = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                
            }
            catch (Exception ex)
            {
                // I/O errors are reported as exceptions.
                
            }
        }
    }
}
