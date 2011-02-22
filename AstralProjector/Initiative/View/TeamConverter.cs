using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Astral.Projector.Initiative.View
{
    public class TeamConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Team team = (Team)value;
            switch (team)
            {
                case Team.Blue:
                    return "/Images/blue.ico";
                case Team.Gold:
                    return "/Images/gold.ico";
                case Team.Green:
                    return "/Images/green.ico";
                case Team.Purple:
                    return "/Images/purple.ico";
                case Team.Red:
                    return "/Images/red.ico";
                case Team.RedGold:
                    return "/Images/redgold.ico";
                case Team.Silver:
                    return "/Images/silver.ico";
                case Team.BlueFlag:
                    return "/Images/Flag_Blue.png";
                case Team.GreenFlag:
                    return "/Images/Flag_Green.png";
                case Team.PurpleFlag:
                    return "/Images/Flag_Purple.png";
                case Team.RedFlag:
                    return "/Images/Flag_Red.png";
                case Team.YellowFlag:
                    return "/Images/Flag_Yellow.png";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
