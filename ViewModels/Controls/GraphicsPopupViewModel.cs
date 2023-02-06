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

namespace map_app.ViewModels.Controls
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

            ShowAddEditGraphicDialog = new Interaction<GraphicAddEditViewModel, GraphicsPopupViewModel>();
            
            OpenEditGraphicView = ReactiveCommand.CreateFromTask(async () =>
            {
                var manager = new GraphicAddEditViewModel(SelectedGraphic!);
                var result = await ShowAddEditGraphicDialog.Handle(manager);
            }, canExecute);           

            OpenAddGraphicView = ReactiveCommand.CreateFromTask<GraphicType>(async (type) =>
            {
                var manager = new GraphicAddEditViewModel(_savedGraphicLayer, type);
                var result = await ShowAddEditGraphicDialog.Handle(manager);
            });
        }

        public Interaction<GraphicAddEditViewModel, GraphicsPopupViewModel> ShowAddEditGraphicDialog { get; }

        public Image ArrowImage => _arrowImage.Value;

        public ObservableCollection<BaseGraphic> Graphics { get; }

        [Reactive]
        public BaseGraphic? SelectedGraphic { get; set; }

        [Reactive]
        public bool IsGraphicsListOpen { get; set; }

        public ICommand IsGraphicsListPressed { get; }

        public ICommand RemoveGraphic { get; }

        public ICommand OpenEditGraphicView { get; }

        public ICommand OpenAddGraphicView { get; }

        private IEnumerable<BaseGraphic> GetSavedGraphics => _savedGraphicLayer!
                .GetFeatures()
                .Cast<BaseGraphic>();
    }
}