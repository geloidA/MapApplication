using Avalonia.ReactiveUI;
using map_app.ViewModels;

namespace map_app.Views;

public partial class GraphicAddEditView : ReactiveWindow<GraphicAddEditViewModel>
{
    public GraphicAddEditView()
    {
        InitializeComponent();
    }
}