using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Avalonia.Controls;
using Avalonia.Input;
using map_app.Services;
using Mapsui.Layers;

namespace map_app.ViewModels
{
    public class AddLayerViewModel : ViewModelBase
    {
        private readonly Map _map;

        public AddLayerViewModel(Map map)
        {
            _map = map;
            Cancel = ReactiveCommand.Create<ICloseable>(CommonFunctionality.CloseView);
            Confirm = ReactiveCommand.Create(() => 
            {
                _map.Layers.Add(new Layer { Name = "User" + this.Name!, Opacity = this.Opacity }); // todo: think how get data source
            }
            , this.WhenAnyValue(
                x => x.Name, x => x.Opacity, 
                (name, opacity) => 
                    !string.IsNullOrEmpty(name) && opacity >= 0 && opacity <= 1));
        }

        [Reactive]
        public double Opacity { get; set; }

        [Reactive]
        public string? Name { get; set; }

        public ICommand Confirm { get; }
        public ICommand Cancel { get; }
    }
}