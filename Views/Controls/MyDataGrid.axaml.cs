using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using map_app.Services;

namespace map_app.Views.Controls;

public partial class MyDataGrid : DataGrid, IStyleable
{
    Type IStyleable.StyleKey => typeof(DataGrid);
    
    public static readonly StyledProperty<Cell> CurrentCellProperty =
        AvaloniaProperty.Register<DataGrid, Cell>(nameof(CurrentCell));

    public Cell CurrentCell
    {
        get { return GetValue(CurrentCellProperty); }
        set { SetValue(CurrentCellProperty, value); }
    }

    public MyDataGrid()
    {
        InitializeComponent();
    }

    protected override void OnCellEditEnded(DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            CurrentCell = new Cell 
            { 
                Column = e.Column.DisplayIndex,
                Row = e.Row.GetIndex()
            };
        }
        base.OnCellEditEnded(e);
    }
}