using Avalonia.Controls;

namespace map_app.Services;

public static class WindowCloser
{
    public static void Close(Window view) => view.Close();

    public static void Close(Window view, object? dialogResult) => view.Close(dialogResult);
}