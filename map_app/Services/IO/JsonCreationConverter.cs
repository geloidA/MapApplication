using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace map_app.Services.IO;

public abstract class JsonCreationConverter<T> : JsonConverter
{
    /// <summary>
    /// Create an instance of objectType, based properties in the JSON object
    /// </summary>
    /// <param name="objectType">type of object expected</param>
    /// <param name="jObject">
    /// contents of JSON object that will be deserialized
    /// </param>
    /// <returns></returns>
    protected abstract T Create(Type objectType, JObject jObject);

    public override bool CanConvert(Type objectType)
        => typeof(T).IsAssignableFrom(objectType);

    public override bool CanWrite => false;

    public override object ReadJson(JsonReader reader, 
                                    Type objectType, 
                                    object? existingValue, 
                                    JsonSerializer serializer)
    {
        // Load JObject from stream
        var jObject = JObject.Load(reader);
        // Create target object based on JObject
        var target = Create(objectType, jObject);
        // Populate the object properties
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }
}