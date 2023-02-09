namespace map_app.Models
{
    public interface IThreeDimensionalPoint
    {
        /// <summary>
        ///  Lon or X
        /// </summary>       
        public double First { get; set; }

        /// <summary>
        ///  Lat or Y
        /// </summary>  
        public double Second { get; set; }

        /// <summary>
        ///  Alt or Z
        /// </summary>  
        public double Third { get; set; }
    }
}