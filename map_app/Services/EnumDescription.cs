namespace map_app.Services;

public record EnumDescription
{
    public object? Value { get; set; }

    public string? Description { get; set; }
    
    public string? Help { get; set; }
    
    public override string? ToString() => Description;
}