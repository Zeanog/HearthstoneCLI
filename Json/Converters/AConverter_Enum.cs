using Newtonsoft.Json;
using System;

public abstract class AConverter_Enum<TEnum> : JsonConverter {
    public override bool CanRead {
        get {
            return true;
        }
    }
    public override bool CanWrite {
        get {
            return true;
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TEnum);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string name = reader.Value.ToString();
        return Enum.Parse(typeof(TEnum), name);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value.ToString());
    }
}