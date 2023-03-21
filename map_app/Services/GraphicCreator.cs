using System;
using System.Collections.Generic;
using map_app.Models;
using NetTopologySuite.Geometries;

namespace map_app.Services;

public static class GraphicCreator
{
    public static BaseGraphic Create(GraphicType type)
    {
        return type switch
        {
            GraphicType.Orthodrome => new OrthodromeGraphic(),
            GraphicType.Point => new PointGraphic(),
            GraphicType.Polygon => new PolygonGraphic(),
            GraphicType.Rectangle => new RectangleGraphic(),
            _ => throw new NotImplementedException("Unknown graphicType")
        };
    }

    public static BaseGraphic Create(GraphicType target, List<Coordinate> source)
    {
        return target switch
        {
            GraphicType.Orthodrome => new OrthodromeGraphic(source),
            GraphicType.Point => new PointGraphic(source),
            GraphicType.Polygon => new PolygonGraphic(source),
            GraphicType.Rectangle => new RectangleGraphic(source),
            _ => throw new NotImplementedException("Unknown graphicType")
        };
    }            
}