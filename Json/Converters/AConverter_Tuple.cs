using Newtonsoft.Json;
using System;

public abstract class AConverter_Tuple<TValue1, TValue2> : JsonConverter {
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
        return objectType == typeof(Tuple<TValue1, TValue2>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value.ToString());
    }
}

public class Converter_TupleFloatFloat : AConverter_Tuple<float, float> {
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string encodedData = reader.Value.ToString();
        string[] values = encodedData.Split(',');
        return new Tuple<float, float>(float.Parse(values[0].Trim()), float.Parse(values[1].Trim()));
    }
}

public class Converter_TupleIntInt : AConverter_Tuple<int, int> {
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        string encodedData = reader.Value.ToString();
        string[] values = encodedData.Split(',');
        return new Tuple<int, int>(int.Parse(values[0].Trim()), int.Parse(values[1].Trim()));
    }
}