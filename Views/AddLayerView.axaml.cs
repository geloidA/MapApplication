using Avalonia.Controls;
using Avalonia.ReactiveUI;
using map_app.ViewModels;

namespace map_app.Views;

public partial class AddLayerView : ReactiveWindow<AddLayerViewModel>
{
    private readonly string _txtBoxTip = @"Введите известный домен
или маску получения плиток.
Например ""http://domen/tile/{z}/{x}/{y}.png""";

    public AddLayerView()
    {
        InitializeComponent();
        TxtBoxAddress.PointerMoved += (_, __) =>
        {
            ToolTip.SetTip(TxtBoxAddress, _txtBoxTip);
        };
    }
}