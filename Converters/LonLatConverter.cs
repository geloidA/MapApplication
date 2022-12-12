using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace map_app.Converters
{
    public class LonLatConverter
    {
        public static (double Lon, double Lat) ConvertFrom(string line, params string[] separator)
        {
            var coordinates = line.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse)
                    .ToArray();

            if (coordinates.Length != 2)
            {
                throw new ArgumentException();
            }
            return (coordinates[0], coordinates[1]);
        }        
    }
}