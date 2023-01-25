using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ReactiveUI;

namespace map_app.Views.Controls;

public class CustomColorPicker : TemplatedControl
{
    private Color _selectedColor;
    private ImmutableSolidColorBrush _selectedBackground;
    private ICommand _selectColorCommand;

    public static readonly DirectProperty<CustomColorPicker, Color> SelectedColorProperty =
        AvaloniaProperty.RegisterDirect<CustomColorPicker, Color>(
            nameof(SelectedColor),
            o => o.SelectedColor,
            (o, v) => o.SelectedColor = v,
            defaultBindingMode: BindingMode.OneWayToSource);

    public Color SelectedColor
    {
        get { return _selectedColor; }
        set { SetAndRaise(SelectedColorProperty, ref _selectedColor, value); }
    }

    public static readonly DirectProperty<CustomColorPicker, ImmutableSolidColorBrush> SelectedBackgroundProperty =
        AvaloniaProperty.RegisterDirect<CustomColorPicker, ImmutableSolidColorBrush>(
            nameof(SelectedBackground),
            o => o.SelectedBackground,
            (o, v) => o.SelectedBackground = v,
            defaultBindingMode: BindingMode.OneWayToSource);

    public ImmutableSolidColorBrush SelectedBackground
    {
        get { return _selectedBackground; }
        set { SetAndRaise(SelectedBackgroundProperty, ref _selectedBackground, value); }
    }

    public CustomColorPicker()
    {
        _selectColorCommand = ReactiveCommand.Create<ImmutableSolidColorBrush>(brush => 
        {
            SelectedColor = brush.Color;
            SelectedBackground = brush;
        });
    }

    public static readonly StyledProperty<ICommand> SelectColorProperty =
        AvaloniaProperty.Register<CustomColorPicker, ICommand>(nameof(SelectColor));

    public ICommand SelectColor
    {
        get => _selectColorCommand;
        set { SetValue(SelectColorProperty, value); }
    }
}