using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media;
using map_app.Models;
using map_app.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using map_app.Models.Extensions;
using ReactiveUI.Validation.Helpers;
using NetTopologySuite.Geometries;

namespace map_app.ViewModels.Controls
{
    public class AddGraphicViewModel : ReactiveValidationObject
    {
        private OwnWritableLayer _graphicsPool;

        public AddGraphicViewModel(OwnWritableLayer target)
        {
            _graphicsPool = target;
            GraphicTypes = Enum.GetValues(typeof(GraphicType))
                .Cast<GraphicType>();

            AddGraphicObject = ReactiveCommand.Create(() => 
            {
                var graphic = CreateGraphic(CurrentGraphicType, ParseCoordinates(Coordinates));
                graphic.Color = new Mapsui.Styles.Color(Color.R, Color.G, Color.B, Color.A);
                graphic.Opacity = Opacity;
                _graphicsPool.Add(graphic);
            });

            this.ValidationRule(
                vm => vm.Opacity,
                o => o >= 0 && o <= 1,
                "Прозрачность должна быть в пределах от 0 до 1");
        }

        public ICommand AddGraphicObject { get; set; }

        public IEnumerable<GraphicType> GraphicTypes { get; set; }

        [Reactive]
        public GraphicType CurrentGraphicType { get; set; }

        [Reactive]
        public Color Color { get; set; }

        [Reactive]
        public double Opacity { get; set; } // todo: make own text for invalid string exception

        [Reactive]
        public string? Coordinates { get; set; }

        [Reactive]
        public bool IsGeoCoordinates { get; set; }

        private List<Coordinate> ParseCoordinates(string? coordinatesString)
        {
            List<Coordinate> result;
            result = IsGeoCoordinates 
                ? new List<Coordinate>(CoordinateParser.ParseData(
                    coordinatesString, 
                    cortege => new GeoPoint(cortege.X, cortege.Y, cortege.Z))
                        .ToWorldPositions())                        
                : new List<Coordinate>(CoordinateParser.ParseData(
                    coordinatesString,
                    cortege => new LinearPoint(cortege.X, cortege.Y, cortege.Z))
                        .ToCoordinates());
            return result;
        }

        private BaseGraphic CreateGraphic(GraphicType target, List<Coordinate> source) => 
            target switch
                {
                    GraphicType.Orthodrome => new OrthodromeGraphic(source),
                    GraphicType.Point => new PointGraphic(source),
                    GraphicType.Polygon => new PolygonGraphic(source),
                    GraphicType.Rectangle => new RectangleGraphic(source),
                    _ => throw new NotImplementedException()
                };
    }
}