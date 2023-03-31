using Mapsui.Styles;

namespace map_app.Services.Extensions;

public static class VectorStyleExtensions
{
    /// <summary>
    /// Creates deep copy
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static VectorStyle Copy(this VectorStyle source)
    {
        if (source is SymbolStyle symbolStyle) return symbolStyle.Copy();
        var copied = VectorStyleHelper.CopyFields(target: new VectorStyle(), source);        
        return copied;
    }
}