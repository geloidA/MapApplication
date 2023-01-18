using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using map_app.Models;
using Mapsui;

namespace map_app.Editing.Extensions
{
    public static class MPointExtensions
    {
        public static Coordinate3D? ToCoordinate3D(this MPoint? point)
        {
            return point is null
                ? null
                : new Coordinate3D(point.X, point.Y, 0);
        }
    }
}