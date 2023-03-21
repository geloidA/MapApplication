namespace map_app.Models;

public interface IThreeDimensionalPoint
{
    /// <summary>
    ///  Longtitude or X
    /// </summary>       
    public double First { get; set; }

    /// <summary>
    ///  Latitude or Y
    /// </summary>  
    public double Second { get; set; }

    /// <summary>
    ///  Altitude or Z
    /// </summary>  
    public double Third { get; set; }
}