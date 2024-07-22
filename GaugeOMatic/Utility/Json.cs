using Newtonsoft.Json;
using System;
using static GaugeOMatic.Utility.Color;

namespace GaugeOMatic.Utility;

public class Json
{
    private static readonly JsonConverter[] JsonConverters =
    {
        new ColorRGBConverter(),
        new AddRGBConverter()
    };

    internal static JsonSerializerSettings JsonSettings = new()
    {
        Converters = JsonConverters,

        TypeNameHandling = TypeNameHandling.None,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
        Formatting = Formatting.None,
        FloatFormatHandling = FloatFormatHandling.Symbol,
        FloatParseHandling = FloatParseHandling.Double,
        StringEscapeHandling = StringEscapeHandling.Default,
        DefaultValueHandling = DefaultValueHandling.Ignore
    };

    public class ColorRGBConverter : JsonConverter<ColorRGB>
    {
        public override void WriteJson(JsonWriter writer, ColorRGB value, JsonSerializer serializer) => writer.WriteValue((string)value);
        public override ColorRGB ReadJson(JsonReader reader, Type objectType, ColorRGB existingValue, bool hasExistingValue, JsonSerializer serializer) => reader.Value != null ? (string)reader.Value : existingValue;
    }

    public class AddRGBConverter : JsonConverter<AddRGB>
    {
        public override void WriteJson(JsonWriter writer, AddRGB value, JsonSerializer serializer) => writer.WriteValue((string)value);
        public override AddRGB ReadJson(JsonReader reader, Type objectType, AddRGB existingValue, bool hasExistingValue, JsonSerializer serializer) => reader.Value != null ? (string)reader.Value : existingValue;
    }
}
