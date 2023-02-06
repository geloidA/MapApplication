using Avalonia.ReactiveUI;
using map_app.ViewModels;

namespace map_app.Views;

public partial class GraphicEditingView : ReactiveWindow<GraphicAddEditViewModel>
{
    public GraphicEditingView()
    {
        InitializeComponent();
    }
}