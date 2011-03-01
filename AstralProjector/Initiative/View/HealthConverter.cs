using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Astral.Projector.Initiative.View
{
    class HealthConverter : IValueConverter
    {
        public double Width { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Actor actor = value as Actor;
            if (actor != null)
            {
                double pct = actor.CurrentHealth / (double)actor.MaxHealth;
                return Width * pct;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
