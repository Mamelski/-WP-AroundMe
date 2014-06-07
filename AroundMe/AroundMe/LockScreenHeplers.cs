using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace AroundMe
{
    class LockScreenHeplers
    {
        private const string BackgroundRoot = "Images/";
        private const string IconRoot = "Shared/ShellContent/";
        private const string LockScreenData = "LockScreenData.json";
        public static void CleanStorage()
        {
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                TryToDeleteAllFiles(storageFolder, BackgroundRoot);
                TryToDeleteAllFiles(storageFolder, IconRoot);
            }
        }

        private static void TryToDeleteAllFiles(IsolatedStorageFile storageFolder, string directory)
        {
            if (storageFolder.DirectoryExists(directory))
            {
                try
                {
                    var files = storageFolder.GetFileNames(directory);

                    foreach (var file in files)
                    {
                        storageFolder.DeleteFile(directory + file);
                    }
                }
                catch (Exception)
                {
                    // file could be in use
                }
            }
        }

        public static void SaveSelectedBackgroundScreens(List<FlickrImage> data)
        {
            string stringData = JsonConvert.SerializeObject(data);

            using (var storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using(var stream = storageFolder.CreateFile(LockScreenData)){
                    using( StreamWriter writer = new StreamWriter(stream)){
                        writer.Write(stringData);
                    }
                }
            }
        }

        public static void SetRandomImageFromLocakStorage()
        {
            string fileData;
            using(IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication()){
                if(!storageFolder.FileExists(LockScreenData)){
                    return;
                }

                using(IsolatedStorageFileStream stream = storageFolder.OpenFile(LockScreenData, FileMode.Open)){
                    using(StreamReader reader = new StreamReader(stream)){
                        fileData = reader.ReadToEnd();
                    }
                }
            }

            List<FlickrImage> images = JsonConvert.DeserializeObject<List<FlickrImage>>(fileData);

            if(images != null){
                Random rndNumber = new Random();
                int index = rndNumber.Next(images.Count);

                SetImage(images[index].Image1024);
            }

        }

        public static async void SetImage(Uri uri)
        {
            string fileName = uri.Segments[uri.Segments.Length -1];
            string imageName = BackgroundRoot + fileName;
            string iconName = IconRoot + fileName;

            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storageFolder.DirectoryExists(BackgroundRoot))
                {
                    storageFolder.CreateDirectory(imageName);
                }

                if (!storageFolder.FileExists(imageName))
                {
                    using (IsolatedStorageFileStream stream = storageFolder.CreateFile(imageName))
                    {
                        HttpClient client = new HttpClient();
                        var flickResult = await client.GetByteArrayAsync(uri);

                        await stream.WriteAsync(flickResult, 0, flickResult.Length);
                    }

                    storageFolder.CopyFile(imageName, iconName);
                }
            }

            // Set the lockscreen
            SetLockScreen(fileName);
            
        }

        private static async void SetLockScreen(string fileName)
        {
            var hasAccessForLockScreen = LockScreenManager.IsProvidedByCurrentApplication;

            if (!hasAccessForLockScreen)
            {
                var accessRequested =  await LockScreenManager.RequestAccessAsync();

                hasAccessForLockScreen = (accessRequested == LockScreenRequestResult.Granted);
            }

            if (hasAccessForLockScreen)
            {
                Uri imageUri = new Uri("ms-appdata:///local/" + BackgroundRoot + fileName, UriKind.Absolute);
                LockScreen.SetImageUri(imageUri);
            }

            var mainTile = ShellTile.ActiveTiles.FirstOrDefault();

            if (null != mainTile)
            {
                Uri iconUri = new Uri("isostore:///" + IconRoot + fileName, UriKind.Absolute);
                var imgs = new List<Uri>();
                imgs.Add(iconUri);

                CycleTileData tileData = new CycleTileData();
                tileData.CycleImages = imgs;

                mainTile.Update(tileData);
            }

        }
    }
}
