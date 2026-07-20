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
            if (value == null) return GetPlaceholderImage();
            string relativePath = value.ToString();
            string fullPath = Path.IsPathRooted(relativePath)
                    ? relativePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (parameter?.ToString() == "IsGif")
            {
                return relativePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    if (parameter?.ToString() == "AsUri" || relativePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Uri(fullPath, UriKind.Absolute);
                    }

                    using (FileStream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;

                        bitmap.DecodePixelWidth = 550;

                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        return bitmap;
                    }
                }
                catch
                {
                    return GetPlaceholderImage();
                }
            }

            return GetPlaceholderImage();
        }

        private BitmapImage GetPlaceholderImage()
        {
            try
            {
                BitmapImage placeholder = new BitmapImage();
                placeholder.BeginInit();
                placeholder.UriSource = new Uri("pack://application:,,,/Resources/Thumbnail-Placeholder.jpg", UriKind.Absolute);
                placeholder.DecodePixelWidth = 400;
                placeholder.CacheOption = BitmapCacheOption.OnLoad;
                placeholder.EndInit();
                placeholder.Freeze();
                return placeholder;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}