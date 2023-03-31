using Mapsui.Styles;

namespace map_app.Services.Extensions;

public static class SymbolStyleExtensions
{
    /// <summary>
    /// Creates deep copy
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static VectorStyle Copy(this SymbolStyle source)
    {
        var copied = VectorStyleHelper.CopyFields(target: new SymbolStyle(), source);
        copied.SymbolOffset = source.SymbolOffset;
        copied.SymbolScale = source.SymbolScale;
        copied.RotateWithMap = source.RotateWithMap;
        copied.BitmapId = source.BitmapId;
        copied.BlendModeColor = source.BlendModeColor;
        copied.SymbolOffsetRotatesWithMap = source.SymbolOffsetRotatesWithMap;
        copied.UnitType = source.UnitType;
        return copied;
    }
}