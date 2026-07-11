using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace Game_Library
{
    public class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = (string)value;

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                return new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
            }

            return new BitmapImage(new Uri("pack://application:,,,/Resources/Thumbnail-Placeholder.jpg", UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
