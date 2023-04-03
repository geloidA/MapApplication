using Avalonia.Controls;
using Avalonia.Svg;
using DynamicData;
using map_app.Editing;
using map_app.Editing.Extensions;
using map_app.Models;
using map_app.Services;
using map_app.Services.Layers;
using Mapsui.UI.Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace map_app.ViewModels.Controls;

internal class GraphicsPopupViewModel : ViewModelBase
{
    private static readonly Image _arrowRight = new()
    {
        Source = new SvgImage
        {
            Source = SvgSource.Load("Resources/Assets/triangle-right-small.svg", null)
        },
        Width = 15,
        Height = 15
    };

    private static readonly Image _arrowLeft = new()
    {
        Source = new SvgImage
        {
            Source = SvgSource.Load("Resources/Assets/triangle-left-small.svg", null)
        },
        Width = 15,
        Height = 15
    };

    private readonly GraphicsLayer _graphics;
    private readonly MainViewModel _mainVM;
    private readonly ObservableAsPropertyHelper<Image> _arrowImage;
    private readonly ObservableAsPropertyHelper<bool> _isSelectedGraphicNotNull;
    private readonly MapControl _mapControl;
    private bool IsSelectedGraphicNotNull => _isSelectedGraphicNotNull.Value;
    private BaseGraphic? _selectedGraphic;

    public GraphicsPopupViewModel(MainViewModel mainViewModel)
    {
        _mapControl = mainViewModel.MapControl;
        _graphics = mainViewModel.GraphicsLayer;
        _mainVM = mainViewModel;
        _arrowImage = this
            .WhenAnyValue(x => x.IsGraphicsListOpen)
            .Select(isOpen => isOpen ? _arrowLeft : _arrowRight)
            .ToProperty(this, x => x.ArrowImage);

        IsGraphicsListPressed = ReactiveCommand.Create(() => IsGraphicsListOpen ^= true);

        _isSelectedGraphicNotNull = this
            .WhenAnyValue(x => x.SelectedGraphic)
            .Select(g => g is not null)
            .ToProperty(this, x => x.IsSelectedGraphicNotNull);
        var selectedIsNotNull = this.WhenAnyValue(x => x.IsSelectedGraphicNotNull);
        RemoveGraphic = ReactiveCommand.Create(() => _graphics.TryRemove(SelectedGraphic!), canExecute: selectedIsNotNull);

        Graphics = new ObservableCollection<BaseGraphic>(_graphics.Features);
        _graphics.LayersFeatureChanged += OnLayersFeatureChanged;
        OpenEditGraphicView = ReactiveCommand.CreateFromTask(async () =>
            await OpenGraphicView(new GraphicAddEditViewModel(SelectedGraphic!, _mapControl), mainViewModel), canExecute: selectedIsNotNull);

        OpenAddGraphicView = ReactiveCommand.CreateFromTask<GraphicType>(async (type) =>
            await OpenGraphicView(new GraphicAddEditViewModel(_graphics, type, _mapControl), mainViewModel));
        _graphics.LayersFeatureChanged += (_, _) => HaveAnyGraphic = _graphics.Features.Any();
        var haveAnyGraphic = this.WhenAnyValue(x => x.HaveAnyGraphic);
        ClearGraphics = ReactiveCommand.Create(ClearGraphicsImpl, canExecute: haveAnyGraphic);
        CopyGraphic = ReactiveCommand.Create(() => _graphics.Add(SelectedGraphic!.Copy()), selectedIsNotNull);
    }

    private async Task OpenGraphicView(GraphicAddEditViewModel vm, MainViewModel mainViewModel)
    {
        var result = await ShowAddEditGraphicDialog.Handle(vm);
        if (result == DialogResult.OK)
            mainViewModel.DataState = DataState.Unsaved;
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

    internal readonly Interaction<GraphicAddEditViewModel, DialogResult> ShowAddEditGraphicDialog = new();

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
    public ICommand CopyGraphic { get; }
    public ICommand ClearGraphics { get; }
    public ICommand OpenEditGraphicView { get; }
    public ICommand OpenAddGraphicView { get; }

    private void ClearGraphicsImpl()
    {
        _graphics.Clear();
        if (_mainVM.EditMode != EditMode.None && EditMode.DrawingMode.HasFlag(_mainVM.EditMode))
            _mainVM.EditMode = _mainVM.EditMode.GetAddMode();
    }
}