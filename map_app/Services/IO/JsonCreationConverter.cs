using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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

    public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

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
        if (target is null) throw new NullReferenceException("Create failed");
        // Populate the object properties
        serializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }
}