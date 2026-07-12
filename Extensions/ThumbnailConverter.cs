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
            string imagePath = value as string;

            if (string.IsNullOrEmpty(imagePath))
            {
                return GetPlaceholderImage();
            }

            try
            {
                string normalizedPath = imagePath.ToLower().Replace("\\", "/");

                if (normalizedPath.Contains("resources/"))
                {
                    int index = normalizedPath.IndexOf("resources/");
                    string cleanPath = imagePath.Substring(index); 

                    string packUri = $"pack://application:,,,/{cleanPath}";

                    BitmapImage bitmap = new BitmapImage(new Uri(packUri, UriKind.Absolute));
                    return bitmap;
                }

                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi nạp ảnh: " + ex.Message);
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