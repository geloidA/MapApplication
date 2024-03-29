using map_app.Editing.Extensions;
using map_app.Models.Extensions;
using map_app.Services.Attributes;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace map_app.Models;

[Label("Ортодромия")]
public class OrthodromeGraphic : BaseGraphic, IHoveringGraphic
{
    private LinkedList<Orthodrome> _orthodromes = new();

    private OrthodromeGraphic(OrthodromeGraphic source) : base(source)
    {
        _orthodromes = source._orthodromes;
        HoverVertex = source.HoverVertex;
    }

    public Coordinate? HoverVertex { get; set; }

    public OrthodromeGraphic() : base()
    {
    }

    public OrthodromeGraphic(Coordinate startPoint) : base() // for creation via activator
    {
        _coordinates = new List<Coordinate> { startPoint };
        _orthodromes.AddFirst(new Orthodrome(startPoint.ToGeoPoint()));
    }

    public OrthodromeGraphic(List<Coordinate> points) : base()
    {
        if (points.Count < 1)
            throw new ArgumentException("Coordinate count can't be less 1");
        _coordinates = points;
        _orthodromes = CreateOrhodromes(_coordinates
            .ToGeoPoints()
            .ToList());
        RerenderGeometry();
    }

    public OrthodromeGraphic(GeometryFeature geometryFeature) : base(geometryFeature) { }
    public OrthodromeGraphic(Geometry? geometry) : base(geometry) { }

    public override GraphicType Type => GraphicType.Orthodrome;

    public override IReadOnlyList<Coordinate> Coordinates
    {
        get => base.Coordinates;
        set
        {
            _orthodromes = CreateOrhodromes(value.ToGeoPoints().ToList());
            base.Coordinates = value;
        }
    }

    public void AddPoint(Coordinate worldCoordinate)
    {
        var geoWorldCoordinate = worldCoordinate.ToGeoPoint();
        if (_orthodromes.Last != null)
            _orthodromes.Last.Value.End = geoWorldCoordinate;
        _orthodromes.AddLast(new Orthodrome(geoWorldCoordinate));
        _coordinates.Add(worldCoordinate.Copy());
    }

    public override OrthodromeGraphic Copy() => new OrthodromeGraphic(this);

    protected override Geometry RenderGeometry()
    {
        _orthodromes.Last!.Value.End = HoverVertex?.ToGeoPoint() ?? _orthodromes.Last.Value.End; // change last orthodrome point while mouse moving
        return new LineString(_orthodromes
            .SelectMany(x => x.Path)
            .ToWorldPositions()
            .ToArray());
    }

    private static LinkedList<Orthodrome> CreateOrhodromes(List<GeoPoint> points)
    {
        if (points.Count < 2) throw new ArgumentException("Points count must be bigger then 2");
        var orthodromes = new LinkedList<Orthodrome>();
        orthodromes.AddFirst(new Orthodrome(points[0], points[1]));
        foreach (var point in points.Skip(2))
            orthodromes.AddLast(new Orthodrome(orthodromes.Last!.Value.End!, point));
        return orthodromes;
    }
}