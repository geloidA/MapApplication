using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;

namespace map_app.Services
{
    public static class CommonFunctionality
    {        
        public static void CloseView(ICloseable view)
        {
            var wnd = view as Window;
            if (wnd != null)
                wnd.Close();
        }
    }
}