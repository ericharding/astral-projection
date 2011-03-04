using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows;

namespace Astral.Projector.Initiative.View
{
    class HealthConverter : IMultiValueConverter
    {
        public int Width { get; set; }
        
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2) return null;

            double first = System.Convert.ToDouble(values[0]);
            double second = System.Convert.ToDouble(values[1]);
            double result = first / second;
            if (result > 1) result = 1 / result;

            return Width * result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
