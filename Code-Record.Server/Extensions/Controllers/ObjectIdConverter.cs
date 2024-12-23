using Newtonsoft.Json;
using MongoDB.Bson;

namespace Code_Record.Server.Extensions.Controllers;

public class ObjectIdConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ObjectId?) || objectType == typeof(ObjectId);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var value = reader.Value as string;

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return ObjectId.TryParse(value, out var objectId) ? objectId : null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else if (value is ObjectId objectId)
        {
            writer.WriteValue(objectId.ToString());
        }
    }
}
