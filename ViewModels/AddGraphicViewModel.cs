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
using ReactiveUI.Validation.States;
using Avalonia.Media.Immutable;

namespace map_app.ViewModels
{
    public class AddGraphicViewModel : ReactiveValidationObject
    {
        private OwnWritableLayer _graphicsPool;

        public AddGraphicViewModel(OwnWritableLayer target) // todo: change to CoordinateParser.TryParse
        {
            _graphicsPool = target;
            GraphicTypes = Enum.GetValues(typeof(GraphicType))
                .Cast<GraphicType>();

            var canExecute = this.IsValid();

            AddGraphicObject = ReactiveCommand.Create(() => 
            {
                BaseGraphic graphic;
                try
                {
                    graphic = CreateGraphic(CurrentGraphicType, ParseCoordinates(Coordinates));
                    ErrorMessage = string.Empty;
                }
                catch (ArgumentException ex)
                {
                    ErrorMessage = ex.Message;
                    return;
                }
                graphic.Color = new Mapsui.Styles.Color(CurrentColor.R, CurrentColor.G, CurrentColor.B, CurrentColor.A);
                graphic.Opacity = Opacity;
                _graphicsPool.Add(graphic);
                _graphicsPool.DataHasChanged();
            }, canExecute);

            this.ValidationRule(
                vm => vm.Opacity,
                o => o >= 0 && o <= 1,
                "Прозрачность должна быть в пределах от 0 до 1");

            var userNuberPointsCorrect = 
                this.WhenAnyValue(
                    x => x.Coordinates, 
                    x => x.CurrentGraphicType,
                    (c, t) => 
                    {
                        var canParsed = t switch
                        {
                            GraphicType.Orthodrome => CoordinateParser.CanDataParsed(c, l => l >= 2),
                            GraphicType.Point => CoordinateParser.CanDataParsed(c, l => l == 1),
                            GraphicType.Polygon => CoordinateParser.CanDataParsed(c, l => l >= 2),
                            GraphicType.Rectangle => CoordinateParser.CanDataParsed(c, l => l == 2),
                            _ => throw new NotImplementedException()
                        };
                        return canParsed ? ValidationState.Valid : new ValidationState(false, "Неправильное количество точек");
                    });

            this.ValidationRule(x => x.Coordinates, userNuberPointsCorrect);
            ChooseColor = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => CurrentColor = brush.Color);
        }

        public ICommand AddGraphicObject { get; set; }

        public ICommand ChooseColor { get; }

        public IEnumerable<GraphicType> GraphicTypes { get; set; }

        [Reactive]
        public GraphicType CurrentGraphicType { get; set; }

        [Reactive]
        public Color CurrentColor { get; set; }

        [Reactive]
        public string? ErrorMessage { get; set; }

        [Reactive]
        public double Opacity { get; set; }

        [Reactive]
        public string? Coordinates { get; set; } = string.Empty;

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