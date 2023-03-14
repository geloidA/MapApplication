using System;

namespace map_app.Services.Attributes;

public class LabelAttribute : Attribute
{
    public string? Text { get; set; }

    public LabelAttribute(string text)
    {
        Text = text;
    }
}