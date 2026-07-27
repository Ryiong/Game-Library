using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Game_Library.Extensions
{
    public class ThumbnailConverter : IValueConverter
    {
        private static readonly ConcurrentDictionary<string, BitmapImage> _imageCache = new ConcurrentDictionary<string, BitmapImage>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;
            if (string.IsNullOrEmpty(relativePath)) return GetPlaceholderImage();
            string fullPath = Path.IsPathRooted(relativePath)
                    ? relativePath
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (parameter?.ToString() == "IsGif")
            {
                return relativePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
            }

            if (!File.Exists(fullPath)) return null;

            if (_imageCache.TryGetValue(fullPath, out var cachedBitmap))
            {
                return cachedBitmap;
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

                        bitmap.DecodePixelWidth = 330;

                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        _imageCache.TryAdd(fullPath, bitmap);

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