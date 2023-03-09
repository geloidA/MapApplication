using Avalonia.Controls;
using Avalonia.Input;

namespace map_app.Services
{
    public static class WindowCloser
    {
        public static void Close(ICloseable view)
        {
            if (view is Window wnd) wnd.Close();
        }

        public static void Close(ICloseable view, object? dialogResult)
        {
            if (view is Window wnd) wnd.Close(dialogResult);
        }
    }
}