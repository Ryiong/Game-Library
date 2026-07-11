using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace Game_Library.Extensions
{
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = (string)value;

            if (!string.IsNullOrEmpty(imagePath))
            {
                return GetPlaceholderImage();
            }
            
            try
            {
                if (imagePath.StartsWith("./Resources/") || imagePath.StartsWith("Resources/"))
                {
                    string cleanPath = imagePath.Replace("./", "");
                    string packUri = $"pack://application:,,,/{cleanPath}";

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);

                    bitmap.DecodePixelHeight = 450;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    return bitmap;
                }

                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);

                    bitmap.DecodePixelHeight = 450;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));

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
