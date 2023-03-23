using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Mapsui;
using System.Reactive.Linq;
using Mapsui.Layers;
using ReactiveUI;
using map_app.Services;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using DynamicData.Binding;
using Avalonia.Controls;

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
        
        SaveAndClose = ReactiveCommand.Create<Window>(WindowCloser.Close);
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
}