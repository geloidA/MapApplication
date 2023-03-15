using System;

namespace map_app.Services.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public sealed class LabelAttribute : Attribute
{
    public string? Text { get; set; }

    public LabelAttribute(string text)
    {
        Text = text;
    }
}