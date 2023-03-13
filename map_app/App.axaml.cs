using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using map_app.Views;
using Microsoft.Extensions.Configuration;
using Splat.ModeDetection;

namespace map_app;

public partial class App : Application
{
    public static readonly string ImportImagesLocation;
    public static readonly IConfigurationRoot Config;

    static App()
    {
        ImportImagesLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ImportedImages");
        if (!Directory.Exists(ImportImagesLocation))
            Directory.CreateDirectory(ImportImagesLocation);
        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional:true, reloadOnChange: true)
            .Build();
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}