using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace map_app.Services;

public class ObservableStack<T> : ObservableCollection<T> //todo: make limited
{
    public ObservableStack()
    {
    }

    public ObservableStack(IEnumerable<T> collection)
    {
        foreach (var item in collection)
            Push(item);
    }

    public T Pop()
    {
        var item = base[base.Count - 1];
        base.RemoveAt(base.Count - 1);
        return item;
    }

    public void Push(T item) => base.Add(item);
}