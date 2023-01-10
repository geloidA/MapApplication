using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using map_app.ViewModels;

namespace map_app.Views;

public partial class AddLayerView : ReactiveWindow<AddLayerViewModel>
{
    public AddLayerView()
    {
        InitializeComponent();
    }
}