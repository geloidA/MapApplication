using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media;
using map_app.Models;
using map_app.Services;
using NetTopologySuite.Geometries;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using map_app.Models.Extensions;
using ReactiveUI.Validation.Helpers;

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
                CurrentGraphicType switch
                {
                    GraphicType.Orthodrome => _graphicsPool.Add(new OrthodromeGraphic() 
                    {
                         Color = new Mapsui.Styles.Color(Color.R, Color.G, Color.B, Color.A),
                         Opacity = Opacity
                    }),
                    GraphicType.Point => _graphicsPool.Add(new PointGraphic()),
                    GraphicType.Polygon => _graphicsPool.Add(new PolygonGraphic()),
                    GraphicType.Rectangle => _graphicsPool.Add(new RectangleGraphic())
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
        public double Opacity { get; set; } // todo: make own text for not number exception

        [Reactive]
        public string? Coordinates { get; set; }

        [Reactive]
        public bool IsGeoCoordinates { get; set; }

        private List<Coordinate> GetCoorditanes()
        {
            var result = new List<Coordinate>();
            result = IsGeoCoordinates 
                ? new List<Coordinate>(CoordinateParser.ParseGeoData(Coordinates).ToWorldPositions())
                : new List<Coordinate>(CoordinateParser.ParseLinearData(Coordinates).ToCoordinates()); // todo: need save Z Coordinate is haven't
            return result;
        }
    }
}