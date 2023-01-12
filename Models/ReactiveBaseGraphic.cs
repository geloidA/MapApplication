using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace map_app.Models
{
    public class ReactiveBaseGraphic : ReactiveObject
    {
        private readonly BaseGraphic _graphic;
        private double _opacity;

        public ReactiveBaseGraphic(BaseGraphic graphic)
        {
            _graphic = graphic;
            _opacity = _graphic.Opacity;
        }

        public double Opacity
        {
            get => _opacity;
            set => this.RaiseAndSetIfChanged(ref _opacity, value);
        }
    }
}