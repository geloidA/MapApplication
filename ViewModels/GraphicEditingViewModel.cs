#region Usings
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
using System.Text;
#endregion

namespace map_app.ViewModels
{
    public class GraphicEditingViewModel : ReactiveValidationObject
    {
        private BaseGraphic _editGraphic;
        private List<LinearPoint> _linear;
        private List<GeoPoint> _geo;
        private readonly bool _isAddMode;
        private readonly OwnWritableLayer? _graphicPool;

        public GraphicEditingViewModel(OwnWritableLayer target, GraphicType pointType) : this(GraphicCreator.Create(pointType)) 
        { 
            _graphicPool = target;
            _isAddMode = true; 
        }

        public GraphicEditingViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;
            PointTypes = EnumUtils.ToDescriptions(typeof(PointType));
            _linear = new List<LinearPoint>(_editGraphic.LinearPoints);
            _geo = new List<GeoPoint>(_editGraphic.GeoPoints);
            Points = new ObservableCollection<IThreeDimensionalPoint>(_linear);
            Tags = new ObservableCollection<Tag>(_editGraphic.UserTags?.Select(x => new Tag() { Name = x.Key, Value = x.Value })
                ?? new List<Tag>());
            Opacity = _editGraphic.Opacity;
            GraphicType = _editGraphic.Type;
            GraphicColor = _editGraphic.Color is null
                ? Colors.Black
                : new Color(
                    r: (byte)_editGraphic.Color.R,
                    g: (byte)_editGraphic.Color.G,
                    b: (byte)_editGraphic.Color.B,
                    a: (byte)_editGraphic.Color.A);
            InitializeCommands();
            this.WhenAnyValue(x => x.SelectedPointType)
                .Subscribe(SwapPointsSource);
            this.WhenAnyValue(x => x.ChangedCell)
                .Subscribe(ChangeCoordinate);
        }

        private void InitializeCommands()
        {
            var canRemoveTag = this
                .WhenAnyValue(x => x.SelectedTagIndex)
                .Select(x => IsIndexValid(x, Tags.Count));
            RemoveSelectedTag = ReactiveCommand.Create(() => Tags.RemoveAt(SelectedTagIndex), canRemoveTag);
            AddTag = ReactiveCommand.Create(() => Tags.Add(new Tag()));
            var canRemovePoint = this
                .WhenAnyValue(x => x.SelectedPointIndex)
                .Select(x => IsIndexValid(x, Points.Count));
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
                .Select(x => !x.Any() || x.All(y => !y.HasErrors))
                .StartWith(true);
            Cancel = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            SaveChanges = ReactiveCommand.Create<ICloseable>(window =>
            {
                if (HaveValidationErrors(out string message))
                {
                    ShowMessageIncorrectData(message);
                    return;
                }
                ConfirmChanges();
                if (_isAddMode)
                {
                    _graphicPool!.Add(_editGraphic);
                    _graphicPool!.DataHasChanged();
                }
                Cancel.Execute(window);
            }, canSave);
        }

        [Reactive]
        public string? Header1 { get; set; }

        [Reactive]
        public string? Header2 { get; set; }

        [Reactive]
        public string? Header3 { get; set; }

        public ObservableCollection<Tag> Tags { get; }

        [Reactive]
        public int SelectedTagIndex { get; set; }

        public ObservableCollection<IThreeDimensionalPoint> Points { get; }

        [Reactive]
        public PointType SelectedPointType { get; set; } = PointType.Geo;

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

        public ICommand? RemoveSelectedPoint { get; private set; }
        public ICommand? SaveChanges { get; private set; }
        public ICommand? RemoveSelectedTag { get; private set; }
        public ICommand? AddTag { get; private set; }
        public ICommand? Cancel { get; private set; }

        private void ConfirmChanges()
        {
            _editGraphic.Coordinates = _linear.ToCoordinates().ToList();
            var color = GraphicColor;
            _editGraphic.Color = new Mapsui.Styles.Color(red: color.R, green: color.G, blue: color.B, alpha: color.A);
            _editGraphic.Opacity = Opacity;
            _editGraphic.UserTags = Tags.ToDictionary(t => t.Name, t => t.Value ?? string.Empty);
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

        private bool HaveValidationErrors(out string message)
        {
            var errors = new List<string>();
            if (HaveTagDuplicates)
                errors.Add("Имена меток не могут совпадать ");
            if (IsCoordinatesIncorrect)
                errors.Add("Некорректные координаты ");
            if (!IsCorrectCoordinateNumber)
                errors.Add("Некорректное количество координат ");
            message = string.Join('\n', errors);
            return errors.Any();
        }

        private bool HaveTagDuplicates
            => Tags.GroupBy(x => x.Name)
                .Any(g => g.Count() > 1);

        private bool IsCoordinatesIncorrect 
            => _geo.Where(g => IsCoordinateIncorrect(g))
                .Any();

        private bool IsCoordinateIncorrect(GeoPoint point) // need check z and altitude
        {
            return point.Longtitude < -180 
                || point.Longtitude > 180
                || point.Latitude < -85
                || point.Latitude > 85;
        }

        private bool IsCorrectCoordinateNumber        
            => _editGraphic switch
            {
                PointGraphic => _geo.Count == 1,
                RectangleGraphic => _geo.Count == 2,
                PolygonGraphic => _geo.Count >= 2,
                OrthodromeGraphic => _geo.Count >= 2,
                _ => throw new NotImplementedException()
            };        

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
                    SetNames(new[] { "X", "Y", "Z" });
                    break;
                case PointType.Geo:
                     SetNames(new[] { "Долгота", "Широта", "Высота" });
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

        private static bool IsIndexValid(int index, int count) => index != -1 && index <= count - 1;
    }
}