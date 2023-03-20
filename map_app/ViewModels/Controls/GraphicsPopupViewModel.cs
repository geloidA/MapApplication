using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Svg;
using DynamicData;
using map_app.Models;
using map_app.Services.Layers;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.ViewModels.Controls;

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

    private readonly GraphicsLayer _graphics;
    private readonly ObservableAsPropertyHelper<Image> _arrowImage;
    private readonly ObservableAsPropertyHelper<bool> _isSelectedGraphicNotNull;
    private readonly MapControl _mapControl;
    private bool IsSelectedGraphicNotNull => _isSelectedGraphicNotNull.Value;
    private BaseGraphic? _selectedGraphic;
    
    public GraphicsPopupViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _graphics = mainViewModel.Graphics;
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
        RemoveGraphic = ReactiveCommand.Create(() => _graphics.TryRemove(SelectedGraphic!), canExecute);
        
        Graphics = new ObservableCollection<BaseGraphic>(_graphics.Features);
        _graphics.LayersFeatureChanged += OnLayersFeatureChanged;
        OpenEditGraphicView = ReactiveCommand.CreateFromTask(async () =>
        {
            var manager = new GraphicAddEditViewModel(SelectedGraphic!);
            await ShowAddEditGraphicDialog.Handle(manager);
        }, canExecute);           

        OpenAddGraphicView = ReactiveCommand.CreateFromTask<GraphicType>(async (type) =>
        {
            var manager = new GraphicAddEditViewModel(_graphics, type);
            await ShowAddEditGraphicDialog.Handle(manager);
        });
        _graphics.LayersFeatureChanged += (_, _) => HaveAnyGraphic = _graphics.Features.Any();
        var haveAnyGraphic = this.WhenAnyValue(x => x.HaveAnyGraphic);
        ClearGraphics = ReactiveCommand.Create(() => 
        { 
            _graphics.Clear();
            mainViewModel.MapControl.RefreshGraphics();
        },
        canExecute: haveAnyGraphic);
    }

    private void OnLayersFeatureChanged(object sender, MDataChangedEventArgs args)
    {
        switch (args.Operation)
        {
            case CollectionOperation.Add:
                Graphics.Add(args.Values.First());
                break;
            case CollectionOperation.Remove:
                Graphics.Remove(args.Values.First());
                break;
            case CollectionOperation.AddRange:
                Graphics.AddRange(args.Values);
                break;
            case CollectionOperation.Clear:
                Graphics.Clear();
                break;            
        }
    }

    internal readonly Interaction<GraphicAddEditViewModel, Unit> ShowAddEditGraphicDialog = new();

    public Image ArrowImage => _arrowImage.Value;

    public ObservableCollection<BaseGraphic> Graphics { get; }

    [Reactive]
    private bool HaveAnyGraphic { get; set; }

    public BaseGraphic? SelectedGraphic
    {
        get => _selectedGraphic;
        set
        {
            if (value is null)
                return;
            this.RaiseAndSetIfChanged(ref _selectedGraphic, value);
            _mapControl.Navigator!.CenterOn(_selectedGraphic!.Extent!.Centroid);
        }
    }

    [Reactive]
    public bool IsGraphicsListOpen { get; set; }

    public ICommand IsGraphicsListPressed { get; }

    public ICommand RemoveGraphic { get; }

    public ICommand ClearGraphics { get; }

    public ICommand OpenEditGraphicView { get; }

    public ICommand OpenAddGraphicView { get; }
}