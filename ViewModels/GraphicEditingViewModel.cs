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

namespace map_app.ViewModels
{
    public class GraphicEditingViewModel : ViewModelBase
    {
        private BaseGraphic _editGraphic;
        private List<LinearPoint> _linear;
        private List<GeoPoint> _geo;

        public GraphicEditingViewModel(BaseGraphic editGraphic)
        {
            _editGraphic = editGraphic;

            PointTypes = EnumUtils.ToDescriptions(typeof(PointType));

            _linear = new List<LinearPoint>(_editGraphic.LinearPoints);
            _geo = new List<GeoPoint>(_editGraphic.GeoPoints);

            Points = new ObservableCollection<IThreeDimensionalPoint>(_linear);

            Opacity = _editGraphic.Opacity;
            GraphicType = _editGraphic.Type;
            GraphicColor = new Color(
                a: (byte)_editGraphic.Color!.A,
                r: (byte)_editGraphic.Color!.R,
                g: (byte)_editGraphic.Color!.G,
                b: (byte)_editGraphic.Color!.B
            );

            var canRemove = this
                .WhenAnyValue(x => x.SelectedIndex)
                .Select(IsIndexValid);

            RemoveSelectedPoint = ReactiveCommand.Create(() => 
            {
                _linear.RemoveAt(SelectedIndex);
                _geo.RemoveAt(SelectedIndex);
                Points.RemoveAt(SelectedIndex);
            }, canRemove);

            this.WhenAnyValue(x => x.SelectedPointType)
                .Subscribe(SwapPoints);
                
            this.WhenAnyValue(x => x.ChangedCell)
                .Subscribe(ChangeCoordinate);
        }

        [Reactive]
        public string? Header1 { get; set; }

        [Reactive]
        public string? Header2 { get; set; }

        [Reactive]
        public string? Header3 { get; set; }

        [Reactive]
        public ObservableCollection<IThreeDimensionalPoint> Points { get; set; }

        [Reactive]
        public PointType SelectedPointType { get; set; } = PointType.Linear;

        [Reactive]
        public int SelectedIndex { get; set; }

        public IEnumerable<EnumDescription> PointTypes { get; }

        [Reactive]
        public Cell? ChangedCell { get; set; }

        [Reactive]
        public double Opacity { get; set; }

        public GraphicType GraphicType { get; set; }

        [Reactive]
        public Color? GraphicColor { get; set; }        

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

        public ICommand RemoveSelectedPoint { get; }

        private void SaveChanges()
        {
            
        }

        private void Cancel(Avalonia.Input.ICloseable window) => WindowCloser.Close(window);

        private void SwapPoints(PointType type)
        {
            ChangeCoordinateHeaders(type);
            Points.Clear();
            IEnumerable<IThreeDimensionalPoint> points = type switch
            {
                PointType.Linear => _linear,
                PointType.Geo => _geo,
                _ => new List<IThreeDimensionalPoint>()
            };
            Points.AddRange(points);
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

        private static bool IsIndexValid(int index) => index != -1;
    }
}