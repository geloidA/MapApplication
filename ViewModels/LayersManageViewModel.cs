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
using Avalonia.Input;

namespace map_app.ViewModels
{
    public class LayersManageViewModel : ViewModelBase
    {
        private readonly ObservableStack<Action> _undoStack;
        private readonly Map _map;

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
            
            SaveAndClose = ReactiveCommand.Create<ICloseable>(WindowCloser.Close);
            Layers = new ObservableCollection<ILayer>(map.Layers.Where(l => l.Name.StartsWith("User")));
            _map.Layers.Changed += (s, e) => { Layers.Clear(); Layers.AddRange(map.Layers.Where(l => l.Name.StartsWith("User"))); };

            #region Commands init

            ShowAddDialog = new Interaction<AddLayerViewModel, LayersManageViewModel>();

            OpenAddLayerView = ReactiveCommand.CreateFromTask(async () => 
            {
                var addView = new AddLayerViewModel(_map, _undoStack);
                var result = await ShowAddDialog.Handle(addView);
            });

            var canExecute = this.WhenAnyValue(x => x.SelectedLayer, x => IsNotNull(x));

            ShowChangeDialog = new Interaction<ChangeLayerViewModel, LayersManageViewModel>();

            OpenChangeLayerView = ReactiveCommand.CreateFromTask(async () =>
            {
                var changeView = new ChangeLayerViewModel(_map, SelectedLayer!, _undoStack);
                var result = await ShowChangeDialog.Handle(changeView);                
            },
            canExecute);

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
            canExecute);

            #endregion
        }

        public ObservableCollection<ILayer> Layers { get; }

        [Reactive]
        public ILayer? SelectedLayer { get; set; }
        
        [ObservableAsProperty]
        public bool CanUndo { get; }

        public Interaction<AddLayerViewModel, LayersManageViewModel> ShowAddDialog { get; }
        public Interaction<ChangeLayerViewModel, LayersManageViewModel> ShowChangeDialog { get; }

        public ICommand OpenAddLayerView { get; }
        public ICommand SaveAndClose { get; }
        public ICommand OpenChangeLayerView { get; }
        public ICommand UndoChanges { get; }
        public ICommand RemoveLayer { get; }

        public bool IsNotNull(ILayer? layer) => layer != null;

        private ILayer CopyLayer(ILayer layer) // todo: find better way to copy layer
        {
            return new Layer 
            { 
                Opacity = SelectedLayer!.Opacity, 
                Name = SelectedLayer!.Name, 
                Attribution = new Mapsui.Widgets.Hyperlink 
                { 
                    Url = SelectedLayer!.Attribution.Url
                }
            };
        }
    }
}