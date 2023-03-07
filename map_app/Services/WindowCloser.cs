using Avalonia.Controls;
using Avalonia.Input;

namespace map_app.Services
{
    public static class WindowCloser
    {
        public static void Close(ICloseable view)
        {
            var wnd = view as Window;
            if (wnd != null)
                wnd.Close();
        }
    }
}