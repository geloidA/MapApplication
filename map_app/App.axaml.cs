using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using map_app.Services;
using map_app.Views;
using Newtonsoft.Json;
using Splat.ModeDetection;

namespace map_app;

public partial class App : Application
{
    public static readonly string ImportImagesLocation;
    public static readonly ConfigurationData Configuration;

    static App()
    {
        ImportImagesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ImportedImages");
        if (!Directory.Exists(ImportImagesLocation))
            Directory.CreateDirectory(ImportImagesLocation);
        Configuration = JsonConvert.DeserializeObject<ConfigurationData>(File.ReadAllText("appsettings.json"))
            ?? throw new NullReferenceException($"appsettings.json have wrong format. Need be suited with properties {nameof(ConfigurationData)} class");
    }

    public App()
    {
        Splat.ModeDetector.OverrideModeDetector(Mode.Run);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ExpressionObserver.DataValidators.RemoveAll(x => x is DataAnnotationsValidationPlugin);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}