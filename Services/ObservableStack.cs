using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace map_app.Services
{

    public class ObservableStack<T> : ObservableCollection<T>
    {
        public ObservableStack()
        {
        }

        public ObservableStack(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Push(item);
        }

        public ObservableStack(List<T> list)
        {
            foreach (var item in list)
                Push(item);
        }

        public T Pop()
        {
            var item = base[base.Count - 1];
            base.RemoveAt(base.Count - 1);
            return item;
        }

        public void Push(T item)
        {
            base.Add(item);
        }
    }
}