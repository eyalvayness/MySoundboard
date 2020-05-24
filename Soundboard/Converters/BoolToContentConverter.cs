using Soundboard.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Soundboard.Converters
{
    public class BoolToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            return b ? Resources.Editing : Resources.Remove;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value.ToString() == Resources.Editing;
    }
}
