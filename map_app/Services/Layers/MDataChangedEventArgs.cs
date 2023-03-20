using System;
using System.Collections.Generic;
using System.Linq;
using map_app.Models;

namespace map_app.Services.Layers;

public class MDataChangedEventArgs : EventArgs
{
    public IEnumerable<BaseGraphic> Values { get; }
    public CollectionOperation Operation { get; }

    public MDataChangedEventArgs() : this(CollectionOperation.None, Enumerable.Empty<BaseGraphic>())
    {
    }

    public MDataChangedEventArgs(CollectionOperation operation, IEnumerable<BaseGraphic> values)
    {
        Operation = operation;
        Values = values;
    }
}