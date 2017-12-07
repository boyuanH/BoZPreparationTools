using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BoZPreparation_Tool
{
    public class MapCoordinateRoutedEventArgs : RoutedEventArgs
    {
        public MapCoordinateRoutedEventArgs()
        {

        }
        public string Longitude;
        public string Latitude;
    }
}
