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
using System.Reactive;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.Extensions;
using map_app.Services.IO;
using ReactiveUI.Validation.Extensions;
using map_app.Services.Layers;

namespace map_app.ViewModels
{
    public class GraphicAddEditViewModel : ReactiveValidationObject
    {
        private BaseGraphic _editGraphic;
        private List<LinearPoint> _linear;
        private List<GeoPoint> _geo;
        private readonly bool _isAddMode;
        private readonly GraphicsLayer? _graphicPool;

        public GraphicAddEditViewModel(GraphicsLayer target, GraphicType pointType) : this(GraphicCreator.Create(pointType)) 
        { 
            _graphicPool = target;
            _isAddMode = true;
        }

        public GraphicAddEditViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;
            PointTypes = EnumUtils.ToDescriptions(typeof(PointType));
            _linear = new List<LinearPoint>(_editGraphic.LinearPoints);
            _geo = new List<GeoPoint>(_editGraphic.GeoPoints);
            Points = new ObservableCollection<IThreeDimensionalPoint>(_linear);
            Tags = new ObservableCollection<UserTag>(_editGraphic.UserTags?.Select(x => new UserTag() { Name = x.Key, Value = x.Value })
                ?? new List<UserTag>());
            Opacity = _editGraphic.Opacity;
            GraphicName = _editGraphic.Name;
            GraphicType = _editGraphic.Type;            
            GraphicColor = _editGraphic.StyleColor is null
                ? Colors.Black
                : new Avalonia.Media.Color(
                    r: (byte)_editGraphic.StyleColor.R,
                    g: (byte)_editGraphic.StyleColor.G,
                    b: (byte)_editGraphic.StyleColor.B,
                    a: (byte)_editGraphic.StyleColor.A);
            ShowOpenFileDialog = new Interaction<List<string>, string?>();
            InitializeCommands();
            this.WhenAnyValue(x => x.SelectedPointType)
                .Subscribe(SwapPointsSource);
            this.WhenAnyValue(x => x.ChangedCell)
                .Subscribe(ChangeCoordinate);
            if (_editGraphic is PointGraphic point)
            {
                ImagePath = point.Image;
                PointScale = ((SymbolStyle)point.GraphicStyle).SymbolScale;
            }
            this.ValidationRule(x => x.PointScale,
                                scale => scale > 0 && scale <= 1,
                                "Размер должен быть в диапозоне от 0 до 1");
        }

        private void InitializeCommands()
        {
            var canRemoveTag = this
                .WhenAnyValue(x => x.SelectedTagIndex)
                .Select(x => IsIndexValid(x, Tags.Count));
            RemoveSelectedTag = ReactiveCommand.Create(() => Tags.RemoveAt(SelectedTagIndex), canRemoveTag);
            AddTag = ReactiveCommand.Create(() => Tags.Add(new UserTag()));
            var canRemovePoint = this
                .WhenAnyValue(x => x.SelectedPointIndex)
                .Select(x => IsIndexValid(x, Points.Count));
            RemoveSelectedPoint = ReactiveCommand.Create(() => 
            {
                _linear.RemoveAt(SelectedPointIndex);
                _geo.RemoveAt(SelectedPointIndex);
                Points.RemoveAt(SelectedPointIndex);
            }, canRemovePoint);
            var isTagsValid = Tags
                .ToObservableChangeSet()
                .AutoRefresh(m => m.Name)
                .ToCollection()
                .Select(x => !x.Any() || x.All(y => !y.HasErrors))
                .StartWith(true);
            Close = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            SaveChanges = ReactiveCommand.Create<ICloseable>(SaveChangesImpl, Observable.Merge(this.IsValid(), isTagsValid));
            SelectImageAsync = ReactiveCommand.CreateFromTask(SelectImageAsyncImpl);
            var isImageInit = this
                .WhenAnyValue(x => x.ImagePath)
                .Select(x => !string.IsNullOrEmpty(x));
            RemoveImage = ReactiveCommand.Create(() => ImagePath = null, isImageInit);
        }

        [Reactive]
        public string? CoordinateHeader1 { get; set; }

        [Reactive]
        public string? CoordinateHeader2 { get; set; }

        [Reactive]
        public string? CoordinateHeader3 { get; set; }

        [Reactive]
        public string? ImagePath { get; set; }

        [Reactive]
        public double PointScale { get; set; } = 1;

        [Reactive]
        public string? GraphicName { get; set; }

        public ObservableCollection<UserTag> Tags { get; }

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
        public Avalonia.Media.Color GraphicColor { get; set; }

        internal Interaction<List<string>, string?> ShowOpenFileDialog { get; }

        public ICommand? RemoveSelectedPoint { get; private set; }
        public ICommand? RemoveImage { get; private set; }
        public ICommand? SaveChanges { get; private set; }
        public ICommand? RemoveSelectedTag { get; private set; }
        public ICommand? AddTag { get; private set; }
        public ICommand? Close { get; private set; }
        public ICommand? SelectImageAsync { get; private set; }

        private async Task SelectImageAsyncImpl()
        {
            var imagePath = await ShowOpenFileDialog.Handle(new List<string> { "png", "webp", "jpg", "jpeg" });
            if (imagePath is null)
                return;
            ImagePath = imagePath;
        }

        private void SaveChangesImpl(ICloseable wnd)
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
            Close?.Execute(wnd);
        }

        private void ConfirmChanges()
        {
            _editGraphic.Coordinates = _linear.ToCoordinates().ToList();
            var color = GraphicColor;
            if (_editGraphic is PointGraphic point)
            {
                point.Scale = PointScale;
                 UpdatePointStyle(point);
            }
            _editGraphic.StyleColor = new Mapsui.Styles.Color(red: color.R, green: color.G, blue: color.B, alpha: color.A);
            _editGraphic.Opacity = Opacity;
            _editGraphic.Name = GraphicName;
            _editGraphic.UserTags = Tags.ToDictionary(t => t.Name, t => t.Value ?? string.Empty);
                       
        }

        private async void UpdatePointStyle(PointGraphic point)
        {
            if (point.Image != ImagePath)
            {
                var style = await GetNewPointStyle(point);
                if (style is null) 
                    ShowMessageIncorrectData("Файла нет");
                else
                    point.GraphicStyle = style;
            }
        }

        private async Task<VectorStyle?> GetNewPointStyle(PointGraphic point)
        {
            point.Image = ImagePath;
            if (point.Image is null) return new SymbolStyle();
            var bitmapId = await ImageRegister.RegisterAsync(point.Image);
            return bitmapId is not null ? new SymbolStyle { BitmapId = bitmapId.Value } : null;
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
            => Tags
                .GroupBy(x => x.Name)
                .Any(g => g.Count() > 1);

        private bool IsCoordinatesIncorrect 
            => _geo
                .Where(g => IsCoordinateIncorrect(g))
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
            CoordinateHeader1 = names[0];
            CoordinateHeader2 = names[1];
            CoordinateHeader3 = names[2];
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