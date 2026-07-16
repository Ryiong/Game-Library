using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Game_Library.Extensions
{
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return GetPlaceholderImage();

            string fullPath = path.Replace("\\", "/");

            if (fullPath.StartsWith("games_data/") || fullPath.StartsWith("Resources/"))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string absolutePath = Path.Combine(baseDir, fullPath.Replace("/", "\\"));

                if (File.Exists(absolutePath))
                    path = absolutePath;
            }

            if (File.Exists(path))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch { }
            }

            return GetPlaceholderImage();
        }

        private BitmapImage GetPlaceholderImage()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Thumbnail-Placeholder.jpg", UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}