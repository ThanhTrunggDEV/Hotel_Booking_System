using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Hotel_Booking_System.Converters
{
    public class ImageUrlToBitMap : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> _imageCache = new();
        private static readonly object _cacheLock = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                lock (_cacheLock)
                {
                    if (_imageCache.TryGetValue(url, out var cachedBitmap))
                    {
                        return cachedBitmap;
                    }
                }

                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                    bitmap.EndInit();
                    bitmap.Freeze();

                    lock (_cacheLock)
                    {
                        if (!_imageCache.ContainsKey(url))
                        {
                            _imageCache[url] = bitmap;
                        }
                    }

                    return bitmap;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
