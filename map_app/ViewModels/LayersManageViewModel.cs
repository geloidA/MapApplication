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
using map_app.Services.Layers;
using Avalonia.Controls;

namespace map_app.ViewModels
{
    public class LayersManageViewModel : ViewModelBase
    {
        private readonly Map _map;
        private readonly ObservableStack<Action> _undoStack;

        public LayersManageViewModel(Map map)
        {
            _map = map;
            _undoStack = new ObservableStack<Action>();
            _undoStack
                .ToObservableChangeSet(x => x)
                .ToCollection()
                .Select(items => items.Any())
                .ToPropertyEx(this, x => x.CanUndo); // need for observe changes in observable collection
            UndoChanges = ReactiveCommand.Create(() => _undoStack.Pop()(), this.WhenAnyValue(x => x.CanUndo));
            
            SaveAndClose = ReactiveCommand.Create<Window>(WindowCloser.Close);
            Layers = new ObservableCollection<ILayer>(map.Layers);
            _map.Layers.Changed += (s, e) => 
            { 
                Layers.Clear(); 
                Layers.AddRange(map.Layers.Where(l => l.Tag?.ToString() == "User")); 
                Layers.Add(map.Layers.FindLayer(nameof(GraphicsLayer)));
            };

            #region Commands init

            ShowAddEditDialog = new Interaction<LayerAddEditViewModel, LayersManageViewModel>();

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
                layer => layer != null && layer.Tag?.ToString() != "Graphic");

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

            #endregion
        }

        public ObservableCollection<ILayer> Layers { get; }

        [Reactive]
        public ILayer? SelectedLayer { get; set; }
        
        [ObservableAsProperty]
        public bool CanUndo { get; }

        public Interaction<LayerAddEditViewModel, LayersManageViewModel> ShowAddEditDialog { get; }

        public ICommand OpenLayerAddView { get; }
        public ICommand SaveAndClose { get; }
        public ICommand OpenLayerEditView { get; }
        public ICommand UndoChanges { get; }
        public ICommand RemoveLayer { get; }

        public bool IsNotNull(ILayer? layer) => layer != null;
    }
}