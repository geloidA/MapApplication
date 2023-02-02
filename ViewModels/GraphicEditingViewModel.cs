using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using map_app.Models;
using map_app.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MessageBox.Avalonia;
using System.Collections.Generic;
using System;
using AvaloniaEdit.Utils;
using Mapsui.Projections;
using System.Reactive.Linq;
using ReactiveUI.Validation.Helpers;
using System.Linq;
using map_app.Models.Extensions;
using Avalonia.Input;
using DynamicData.Binding;
using DynamicData;

namespace map_app.ViewModels
{
    public class GraphicEditingViewModel : ReactiveValidationObject
    {
        private BaseGraphic _editGraphic;
        private List<LinearPoint> _linear;
        private List<GeoPoint> _geo;

        private ObservableAsPropertyHelper<bool> _canSaveChanges;

        public GraphicEditingViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;
            PointTypes = EnumUtils.ToDescriptions(typeof(PointType));
            _linear = new List<LinearPoint>(_editGraphic.LinearPoints);
            _geo = new List<GeoPoint>(_editGraphic.GeoPoints);
            Points = new ObservableCollection<IThreeDimensionalPoint>(_linear);
            Tags = new ObservableCollection<Tag>(_editGraphic.UserTags?.Select(x => new Tag() { Name = x.Key, Value = x.Value })
                ?? Array.Empty<Tag>());
            Opacity = _editGraphic.Opacity;
            GraphicType = _editGraphic.Type;
            GraphicColor = new Color(
                a: (byte)_editGraphic.Color!.A,
                r: (byte)_editGraphic.Color!.R,
                g: (byte)_editGraphic.Color!.G,
                b: (byte)_editGraphic.Color!.B
            );

            var canRemoveTag = this
                .WhenAnyValue(x => x.SelectedTagIndex)
                .Select(IsIndexValid);
            RemoveSelectedTag = ReactiveCommand.Create(() => Tags.RemoveAt(SelectedTagIndex), canRemoveTag);
            var canRemovePoint = this
                .WhenAnyValue(x => x.SelectedPointIndex)
                .Select(IsIndexValid);
            RemoveSelectedPoint = ReactiveCommand.Create(() => 
            {
                _linear.RemoveAt(SelectedPointIndex);
                _geo.RemoveAt(SelectedPointIndex);
                Points.RemoveAt(SelectedPointIndex);
            }, canRemovePoint);
            var canSave = Tags
                .ToObservableChangeSet()
                .AutoRefresh(m => m.Name)
                .ToCollection()
                .Select(x => !x.Any() || x.All(y => !y.HasErrors));
            _canSaveChanges = canSave.ToProperty(this, x => x.CanSaveChanges); // dont refresh in first time
            SaveChanges = ReactiveCommand.Create<ICloseable>(window =>
            {
                if (!IsCorrectCoordinateNumber())
                {
                    ShowMessageIncorrectData("Некорректное количество координат ");
                    return;
                }
                if (IsCoordinatesIncorrect)
                {
                    ShowMessageIncorrectData("Некорректные координаты ");
                    return;
                }
                ConfirmChanges();
                Close(window);
            });
            
            this.WhenAnyValue(x => x.SelectedPointType)
                .Subscribe(SwapPointsSource);
                
            this.WhenAnyValue(x => x.ChangedCell)
                .Subscribe(ChangeCoordinate);
        }

        [Reactive]
        public string? Header1 { get; set; }

        [Reactive]
        public string? Header2 { get; set; }

        [Reactive]
        public string? Header3 { get; set; }

        public bool CanSaveChanges => _canSaveChanges.Value;

        public ObservableCollection<Tag> Tags { get; }

        [Reactive]
        public int SelectedTagIndex { get; set; }

        public ObservableCollection<IThreeDimensionalPoint> Points { get; }

        [Reactive]
        public PointType SelectedPointType { get; set; } = PointType.Linear;

        [Reactive]
        public int SelectedPointIndex { get; set; }

        public IEnumerable<EnumDescription> PointTypes { get; }

        [Reactive]
        public Cell? ChangedCell { get; set; }

        [Reactive]
        public double Opacity { get; set; }

        public GraphicType GraphicType { get; set; }

        [Reactive]
        public Color GraphicColor { get; set; }  

        public ICommand RemoveSelectedPoint { get; }

        public ICommand SaveChanges { get; }

        public ICommand RemoveSelectedTag { get; }

        private void ConfirmChanges()
        {
            _editGraphic.Coordinates = _linear.ToCoordinates().ToList();
            var color = GraphicColor;
            _editGraphic.Color = new Mapsui.Styles.Color(red: color.R, green: color.G, blue: color.B, alpha: color.A);
            _editGraphic.Opacity = Opacity;
        }

        private void AddPoint()
        {
            IThreeDimensionalPoint point = SelectedPointType switch
            {
                PointType.Geo => new GeoPoint(),
                PointType.Linear => new LinearPoint(),
                _ => throw new NotImplementedException()
            };
            Points.Add(point);
            if (point is LinearPoint)
            {
                _linear.Add((LinearPoint)point);
                _geo.Add(new GeoPoint());
            }
            else
            {
                _geo.Add((GeoPoint)point);
                _linear.Add(new LinearPoint());
            }
        }

        private void AddTag()
        {
            Tags.Add(new Tag());
        }

        private void Close(ICloseable window) => WindowCloser.Close(window);

        private bool IsCoordinatesIncorrect 
            => _geo.Where(g=> IsCoordinateIncorrect(g))
                .Any();

        private bool IsCoordinateIncorrect(GeoPoint point) // z and altitude
        {
            return point.Longtitude < -180 
                || point.Longtitude > 180
                || point.Latitude < -85
                || point.Latitude > 85;
        }

        private bool IsCorrectCoordinateNumber()
        {
            return _editGraphic switch
            {
                PointGraphic => _geo.Count == 1,
                RectangleGraphic => _geo.Count == 2,
                PolygonGraphic => _geo.Count >= 2,
                OrthodromeGraphic => _geo.Count >= 2,
                _ => throw new NotImplementedException()
            };
        }

        private void SwapPointsSource(PointType targetType)
        {
            ChangeCoordinateHeaders(targetType);
            Points.Clear();
            IEnumerable<IThreeDimensionalPoint> points = targetType switch
            {
                PointType.Linear => _linear,
                PointType.Geo => _geo,
                _ => new List<IThreeDimensionalPoint>()
            };
            Points.AddRange(points);
        }

        private void ChangeCoordinateHeaders(PointType pointType)
        {
            switch (pointType)
            {
                case PointType.Linear:
                    SetNames(new[] { "X", "Y", "Z"});
                    break;
                case PointType.Geo:
                     SetNames(new[] { "Долгота", "Широта", "Высота"});
                     break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SetNames(string[] names)
        {
            Header1 = names[0];
            Header2 = names[1];
            Header3 = names[2];
        }

        private void ChangeCoordinate(Cell? cell)
        {
            if (cell is null)
                return;
            
            switch (cell.Column)
            {
                case 0:
                    ChangeFirstCoordinate(cell);
                    break;
                case 1:
                    ChangeSecondCoordinate(cell);
                    break;
                case 2:
                    ChangeThirdCoordinate(cell);
                    break;
                default: throw new Exception("invalid number of columns");
            }
        }

        private void ChangeThirdCoordinate(Cell cell)
        {
            if (SelectedPointType == PointType.Linear)
                _geo[cell.Row].Altitude = _linear[cell.Row].Z;
            else
                _linear[cell.Row].Z = _geo[cell.Row].Altitude;
        }

        private void ChangeSecondCoordinate(Cell cell)
        {
            if (SelectedPointType == PointType.Linear)
                _geo[cell.Row].Latitude = SphericalMercator.ToLonLat(0, _linear[cell.Row].Y).lat;
            else
                _linear[cell.Row].Y = SphericalMercator.FromLonLat(0, _geo[cell.Row].Latitude).y;
        }

        private void ChangeFirstCoordinate(Cell cell)
        {
            if (SelectedPointType == PointType.Linear)
                _geo[cell.Row].Longtitude = SphericalMercator.ToLonLat(_linear[cell.Row].X, 0).lon;
            else
                _linear[cell.Row].X = SphericalMercator.FromLonLat(_geo[cell.Row].Longtitude, 0).x;
        }

        private void ShowMessageIncorrectData(string message)
        {
            var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                "Некорректные данные",
                message);            
            messageBoxStandardWindow.Show();
        }

        private static bool IsIndexValid(int index) => index != -1;
    }
}