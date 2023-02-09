namespace map_app.Services
{
    public class Cell
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public override string ToString()
        {
            return string.Format("Column:{0} Row:{1}", Column, Row);
        }
    }
}