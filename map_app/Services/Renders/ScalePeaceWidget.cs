using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;

namespace map_app.Services.Renders;

public class ScalePeaceWidget : Widget
    {
    private readonly Map? _map;
    private readonly IProjection? _projection;
    // Instead of using this property we could initialize _projection with ProjectionDefaults.Projection
    // in the constructor but in that way the overriding of ProjectionDefaults.Projection would not have 
    // effect if it was set after the ScaleBarWidget was constructed.
    private IProjection Projection => _projection ?? ProjectionDefaults.Projection;
    ///
    /// Default position of the scale bar.
    ///
    private static readonly HorizontalAlignment DefaultScaleBarHorizontalAlignment = HorizontalAlignment.Left;
    private static readonly VerticalAlignment DefaultScaleBarVerticalAlignment = VerticalAlignment.Bottom;
    private static readonly Alignment DefaultScaleBarAlignment = Alignment.Left;
    private static readonly Font DefaultFont = new() { FontFamily = "Arial", Size = 10 };


    public ScalePeaceWidget(Map map, IProjection? projection = null)
    {
        _map = map;
        _projection = projection;

        HorizontalAlignment = DefaultScaleBarHorizontalAlignment;
        VerticalAlignment = DefaultScaleBarVerticalAlignment;

        _maxWidth = 100;
        _height = 100;
        _textAlignment = DefaultScaleBarAlignment;
        _unitConverter = MetricUnitConverter.Instance;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private float _maxWidth;

    /// <summary>
    /// Maximum usable width for scalebar. The real used width could be less, because we 
    /// want only integers as text.
    /// </summary>
    public float MaxWidth
    {
        get => _maxWidth;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_maxWidth == value)
                return;

            _maxWidth = value;
            OnPropertyChanged();
        }
    }

    private float _height;

    /// <summary>
    /// Real height of scalebar. Depends on number of unit converters and text size.
    /// Is calculated by renderer.
    /// </summary>
    public float Height
    {
        get => _height;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_height == value)
                return;

            _height = value;
            OnPropertyChanged();
        }
    }

    private Color _textColor = new(0, 0, 0);

    /// <summary>
    /// Foreground color of scalebar and text
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor == value)
                return;
            _textColor = value;
            OnPropertyChanged();
        }
    }

    private Color _haloColor = new(255, 255, 255);

    /// <summary>
    /// Halo color of scalebar and text, so that it is better visible
    /// </summary>
    public Color Halo
    {
        get => _haloColor;
        set
        {
            if (_haloColor == value)
                return;
            _haloColor = value;
            OnPropertyChanged();
        }
    }

    public float Scale { get; } = 1;

    /// <summary>
    /// Stroke width for lines
    /// </summary>
    public float StrokeWidth { get; set; } = 2;

    /// <summary>
    /// Stroke width for halo of lines
    /// </summary>
    public float StrokeWidthHalo { get; set; } = 4;

    /// <summary>
    /// Length of the ticks
    /// </summary>
    public float TickLength { get; set; } = 3;

    private Alignment _textAlignment;

    /// <summary>
    /// Alignment of text of scalebar
    /// </summary>
    public Alignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            if (_textAlignment == value)
                return;

            _textAlignment = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Margin between end of tick and text
    /// </summary>
    public float TextMargin => 1;

    private Font? _font = DefaultFont;

    /// <summary>
    /// Font to use for drawing text
    /// </summary>
    public Font? Font
    {
        get => _font ?? DefaultFont;
        set
        {
            if (_font == value)
                return;

            _font = value;
            OnPropertyChanged();
        }
    }

    private IUnitConverter _unitConverter;

    /// <summary>
    /// Normal unit converter for upper text. Default is MetricUnitConverter.
    /// </summary>
    public IUnitConverter UnitConverter
    {
        get => _unitConverter;
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException($"{nameof(UnitConverter)} must not be null");
            }
            if (_unitConverter == value)
            {
                return;
            }

            _unitConverter = value;
            OnPropertyChanged();
        }
    }

    private IUnitConverter? _secondaryUnitConverter;

    /// <summary>
    /// Secondary unit converter for lower text if ScaleBarMode is Both. Default is ImperialUnitConverter.
    /// </summary>
    public IUnitConverter? SecondaryUnitConverter
    {
        get => _secondaryUnitConverter;
        set
        {
            if (_secondaryUnitConverter == value)
            {
                return;
            }

            _secondaryUnitConverter = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Draw a rectangle around the scale bar for testing
    /// </summary>
    public bool ShowEnvelop { get; set; }

    /// <summary>
    /// Calculates the length and text for both scalebars
    /// </summary>
    /// <returns>
    /// Length of upper scalebar
    /// Text of upper scalebar
    /// Length of lower scalebar
    /// Text of lower scalebar
    /// </returns>
    public (float scaleBarLength, string? scaleBarText) GetScaleBarLengthAndText(IReadOnlyViewport viewport)
    {
        if (_map?.CRS == null) 
            return (0, null);
        return CalculateScaleBarLengthAndValue(_map.CRS, Projection, viewport, MaxWidth, UnitConverter);
    }

    /// <summary>
    /// Get pairs of points, which determine start and stop of the lines used to draw the scalebar
    /// </summary>
    /// <param name="viewport">The viewport of the map</param>
    /// <param name="stroke">Width of line</param>
    /// <returns>Array with pairs of Points. First is always the start point, the second is the end point.</returns>
    public IReadOnlyList<MPoint> GetScaleBarLinePositions(IReadOnlyViewport viewport, float scaleBarLength, float stroke)
    {
        var points = new List<MPoint>();

        var posX = CalculatePositionX(0, (int)viewport.Width, _maxWidth);
        var posY = CalculatePositionY(0, (int)viewport.Height, _height);

        var left = posX + stroke * 0.5f * Scale;
        var right = posX + _maxWidth - stroke * 0.5f * Scale;
        var center1 = posX + (_maxWidth - scaleBarLength) / 2;
        // Top position is Y in the middle of scale bar line
        var top = posY + (_height - stroke * 0.5f * Scale);

        switch (TextAlignment)
        {
            case Alignment.Center:
                points.Add(new MPoint(center1, top - TickLength * Scale));
                points.Add(new MPoint(center1, top));
                points.Add(new MPoint(center1, top));
                points.Add(new MPoint(center1 + scaleBarLength, top));
                points.Add(new MPoint(center1 + scaleBarLength, top));
                points.Add(new MPoint(center1 + scaleBarLength, top - TickLength * Scale));
                break;
            case Alignment.Left:
                points.Add(new MPoint(left, top));
                points.Add(new MPoint(left + scaleBarLength, top));
                points.Add(new MPoint(left, top - TickLength * Scale));
                points.Add(new MPoint(left, top));
                points.Add(new MPoint(left + scaleBarLength, top - TickLength * Scale));
                points.Add(new MPoint(left + scaleBarLength, top));
                break;
            case Alignment.Right:
                points.Add(new MPoint(right, top));
                points.Add(new MPoint(right - scaleBarLength, top));
                points.Add(new MPoint(right, top - TickLength * Scale));
                points.Add(new MPoint(right, top));
                points.Add(new MPoint(right - scaleBarLength, top - TickLength * Scale));
                points.Add(new MPoint(right - scaleBarLength, top));
                break;
            default:
                throw new NotSupportedException($"TextAlignment {TextAlignment} is not supported");
        }

        return points;
    }

    /// <summary>
    /// Calculates the top-left-position of upper and lower text
    /// </summary>
    /// <param name="viewport">The viewport</param>
    /// <param name="textSize">Size text of scalebar</param>
    /// <param name="stroke">Width of line</param>
    /// <returns>
    /// posX1 as left position of upper scalebar text
    /// posY1 as top position of upper scalebar text
    /// posX2 as left position of lower scalebar text
    /// posY2 as top position of lower scalebar text
    /// </returns>
    public (float posX1, float posY1) GetScaleBarTextPosition(IReadOnlyViewport viewport, MRect textSize, float stroke)
    {
        var posX = CalculatePositionX(0, (int)viewport.Width, _maxWidth);
        var posY = CalculatePositionY(0, (int)viewport.Height, _height);

        var left = posX + (stroke + TextMargin) * Scale;
        var right = posX + _maxWidth - (stroke + TextMargin) * Scale - (float)textSize.Width;
        var top = posY;

        switch (TextAlignment)
        {
            case Alignment.Center:
                return (posX + (stroke + TextMargin) * Scale + (MaxWidth - 2.0f * (stroke + TextMargin) * Scale - (float)textSize.Width) / 2.0f,
                    top);
            case Alignment.Left:
                return (left, top);
            case Alignment.Right:
                return (right, top);
            default:
                return (0, 0);
        }
    }

    public override bool HandleWidgetTouched(INavigator navigator, MPoint position)
    {
        return false;
    }

    public bool CanProject()
    {
        if (_map?.CRS == null) return false;
        if (Projection == null) return false;
        if (Projection.IsProjectionSupported(_map.CRS, "EPSG:4326") != true) return false;
        return true;
    }


    internal void OnPropertyChanged([CallerMemberName] string name = "")
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// Calculates the required length and value of a scalebar
    ///
    /// @param viewport the Viewport to calculate for
    /// @param width of the scale bar in pixel to calculate for
    /// @param unitConverter the DistanceUnitConverter to calculate for
    /// @return scaleBarLength and scaleBarText
    private static (float scaleBarLength, string scaleBarText) CalculateScaleBarLengthAndValue(
        string CRS, IProjection projection, IReadOnlyViewport viewport, float width, IUnitConverter unitConverter)
    {
        // We have to calc the angle difference to the equator (angle = 0), 
        // because EPSG:3857 is only there 1 m. At other angles, we
        // should calculate the correct length.

        var (_, y) = projection.Project(CRS, "EPSG:4326", viewport.CenterX, viewport.CenterY); // clone or else you will project the original viewport center

        // Calc ground resolution in meters per pixel of viewport for this latitude
        var groundResolution = viewport.Resolution * Math.Cos(y / 180.0 * Math.PI);

        // Convert in units of UnitConverter
        groundResolution = groundResolution / unitConverter.MeterRatio;

        var scaleBarValues = unitConverter.ScaleBarValues;

        float scaleBarLength = 0;
        var scaleBarValue = 0;

        foreach (var value in scaleBarValues)
        {
            scaleBarValue = value;
            scaleBarLength = (float)(scaleBarValue / groundResolution);
            if (scaleBarLength < width - 10)
            {
                break;
            }
        }

        var scaleBarText = unitConverter.GetScaleText(scaleBarValue);

        return (scaleBarLength, scaleBarText);
    }
}