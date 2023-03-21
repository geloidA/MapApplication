namespace map_app.Services;

public class Cell
{
    public int Column { get; }
    public int Row { get; }

    public Cell(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public override string ToString() => $"Column:{Column} Row:{Row}";
}