using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Svg;
using DynamicData;
using map_app.Models;
using map_app.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels
{
    internal class GraphicsPopupViewModel : ViewModelBase
    {
        private static Image _arrowRight = new Image
        {
            Source = new SvgImage
            {
                Source = SvgSource.Load("Resources/Assets/triangle-right-small.svg", null)
            },
            Width = 15,
            Height = 15
        };
        private static Image _arrowLeft = new Image
        {
            Source = new SvgImage
            {
                Source = SvgSource.Load("Resources/Assets/triangle-left-small.svg", null)
            },
            Width = 15,
            Height = 15
        };

        private OwnWritableLayer? _savedGraphicLayer;
        private readonly ObservableAsPropertyHelper<Image> _arrowImage;
        private readonly ObservableAsPropertyHelper<bool> _isSelectedGraphicNotNull;
        private bool IsSelectedGraphicNotNull => _isSelectedGraphicNotNull.Value;
        
        public GraphicsPopupViewModel(OwnWritableLayer savedGraphicLayer)
        {
            _savedGraphicLayer = savedGraphicLayer;
            _arrowImage = this
                .WhenAnyValue(x => x.IsGraphicsListOpen)
                .Select(isOpen => isOpen ? _arrowLeft : _arrowRight)
                .ToProperty(this, x => x.ArrowImage);

            IsGraphicsListPressed = ReactiveCommand.Create(() => IsGraphicsListOpen ^= true);
            
            _isSelectedGraphicNotNull = this
                .WhenAnyValue(x => x.SelectedGraphic)
                .Select(g => g is not null)
                .ToProperty(this, x => x.IsSelectedGraphicNotNull);
            var canExecute = this.WhenAnyValue(x => x.IsSelectedGraphicNotNull);
            RemoveGraphic = ReactiveCommand.Create(() => _savedGraphicLayer.TryRemove(SelectedGraphic!), canExecute);
            
            Graphics = new ObservableCollection<BaseGraphic>(GetSavedGraphics);
            _savedGraphicLayer!.LayersFeatureChanged += (s, e) => 
            {
                Graphics.Clear();
                Graphics?.AddRange(GetSavedGraphics);
            };
        }

        public ObservableCollection<BaseGraphic> Graphics { get; }

        public Image ArrowImage => _arrowImage.Value;

        [Reactive]
        public BaseGraphic? SelectedGraphic { get; set; }

        [Reactive]
        public bool IsGraphicsListOpen { get; set; }

        public ICommand IsGraphicsListPressed { get; }

        public ICommand RemoveGraphic { get; }

        private IEnumerable<BaseGraphic> GetSavedGraphics => _savedGraphicLayer!
                .GetFeatures()
                .Cast<BaseGraphic>();
    }
}