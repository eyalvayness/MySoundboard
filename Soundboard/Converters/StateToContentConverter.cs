using Soundboard.Models;
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
    public class StateToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Sound.PlayingState state = (Sound.PlayingState)value;

            switch (state)
            {
                case Sound.PlayingState.Preview:
                    return Resources.Stop;
                default:
                    return Resources.Preview;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
