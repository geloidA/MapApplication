using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using map_app.Services;
using Mapsui;
using Mapsui.Layers;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace map_app.ViewModels;

public class LayersManageViewModel : ViewModelBase
{
    private readonly Map _map;
    private readonly ObservableStack<Action> _undoStack = new();

    public LayersManageViewModel(Map map)
    {
        _map = map;
        _undoStack
            .ToObservableChangeSet(x => x)
            .ToCollection()
            .Select(items => items.Any())
            .ToPropertyEx(this, x => x.CanUndo); // need for observe changes in observable collection

        SaveAndClose = ReactiveCommand.CreateFromTask<Window>(async window =>
        {
            await SaveTileSourcesInConfig();
            WindowCloser.Close(window);
        });
        var managedLayers = map.Layers.Where(l => l.Tag is ManagedLayerTag);
        Layers = new ObservableCollection<ILayer>(managedLayers);
        _map.Layers.Changed += (s, e) =>
        {
            Layers.Clear();
            Layers.AddRange(managedLayers);
        };
        InitializeCommands();
    }

    private void InitializeCommands()
    {
        UndoChanges = ReactiveCommand.Create(() => _undoStack.Pop()(), this.WhenAnyValue(x => x.CanUndo));

        OpenLayerAddView = ReactiveCommand.CreateFromTask(async () =>
        {
            var view = new LayerAddEditViewModel(_map, _undoStack);
            await ShowAddEditDialog.Handle(view);
        });

        var canExecute = this.WhenAnyValue(x => x.SelectedLayer, IsNotNull);

        OpenLayerEditView = ReactiveCommand.CreateFromTask(async () =>
        {
            var view = new LayerAddEditViewModel(_map, SelectedLayer!, _undoStack);
            await ShowAddEditDialog.Handle(view);
        }, canExecute);

        var canRemove = this
            .WhenAnyValue(x => x.SelectedLayer,
            layer => layer != null && (layer.Tag as ManagedLayerTag)!.CanRemove);

        RemoveLayer = ReactiveCommand.Create(() =>
        {
            var index = _map.Layers.IndexOf(SelectedLayer);
            var copy = SelectedLayer!;
            _map.Layers.Remove(SelectedLayer!);
            _undoStack.Push(() =>
            {
                _map.Layers.Insert(index, copy);
                copy.Dispose();
            });
        },
        canRemove);
    }

    public ObservableCollection<ILayer> Layers { get; }

    [Reactive]
    public ILayer? SelectedLayer { get; set; }

    [ObservableAsProperty]
    public bool CanUndo { get; }

    public Interaction<LayerAddEditViewModel, LayersManageViewModel> ShowAddEditDialog { get; } = new();

    public ICommand? OpenLayerAddView { get; private set; }
    public ICommand? SaveAndClose { get; private set; }
    public ICommand? OpenLayerEditView { get; private set; }
    public ICommand? UndoChanges { get; private set; }
    public ICommand? RemoveLayer { get; private set; }

    public bool IsNotNull(ILayer? layer) => layer != null;

    private Task SaveTileSourcesInConfig()
    {
        App.Configuration.TileSources = Layers
            .Where(x => ((ManagedLayerTag)x.Tag!).HaveTileSource)
            .Select(x => new TileSource 
                { 
                    Name = ((ManagedLayerTag)x.Tag!).Name, 
                    Opacity = x.Opacity,
                    HttpTileSource = x.Attribution.Url
                });
        var configJson = JsonConvert.SerializeObject(App.Configuration, Formatting.Indented);
        return File.WriteAllTextAsync("appsettings.json", configJson);
    }
}