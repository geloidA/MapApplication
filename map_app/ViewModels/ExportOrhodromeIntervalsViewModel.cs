using Avalonia.Controls;
using Avalonia.Input;
using Csv;
using map_app.Models;
using map_app.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace map_app.ViewModels;

public class ExportOrhodromeIntervalsViewModel : ReactiveValidationObject
{
    private readonly OrthodromeGraphic _orthodrome;

    public ExportOrhodromeIntervalsViewModel(OrthodromeGraphic orthodrome)
    {
        _orthodrome = orthodrome;
        this.ValidationRule(x => x.Interval,
                            interval => interval > 0,
                            "Шаг должен быть больше 0");
        Save = ReactiveCommand.Create<Window>(SaveImpl, canExecute: this.IsValid());
        Cancel = ReactiveCommand.Create<Window>(WindowCloser.Close);
    }

    private async void SaveImpl(ICloseable window)
    {
        var saveLocation = await ShowSaveFileDialog.Handle(Unit.Default);
        if (saveLocation is null)
            return;
        var columnNames = new[] { "Lon", "Lat" };
        var csv = await CsvWriter.WriteToTextAsync(columnNames, IntermediatePoints(_orthodrome), ';');
        await File.WriteAllTextAsync(saveLocation, csv);
        Cancel?.Execute(window);
    }

    private IAsyncEnumerable<string[]> IntermediatePoints(OrthodromeGraphic orthodrome)
    {
        return orthodrome.GeoPoints
            .Zip(orthodrome.GeoPoints.Skip(1))
            .SelectMany(pair => MapAlgorithms.GetOrthodromePath(pair.First, pair.Second, Interval))
            .Select(p => new[] { $"{p.Longitude}", $"{p.Latitude}" })
            .ToAsyncEnumerable();
    }

    internal readonly Interaction<Unit, string?> ShowSaveFileDialog = new();

    [Reactive]
    public int Interval { get; set; }

    public ICommand Save { get; }
    public ICommand Cancel { get; }
}