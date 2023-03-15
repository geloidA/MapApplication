using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using map_app.ViewModels;

namespace map_app.Views;

public partial class SettingsView : ReactiveWindow<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}