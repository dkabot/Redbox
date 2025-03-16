using Jayrock.Json;

namespace Redbox.JSONPrettyPrinter
{
    internal sealed class JsonColorWriter : JsonWriter
    {
        public JsonColorWriter(JsonWriter inner)
            : this(inner, null)
        {
        }

        public JsonColorWriter(JsonWriter inner, JsonPalette palette)
        {
            InnerWriter = inner;
            Palette = palette ?? JsonPalette.Auto();
        }

        public JsonWriter InnerWriter { get; }

        public JsonPalette Palette { get; set; }

        public override int Index => InnerWriter.Index;

        public override JsonWriterBracket Bracket => InnerWriter.Bracket;

        public override int Depth => InnerWriter.Depth;

        public override int MaxDepth
        {
            get => InnerWriter.MaxDepth;
            set => InnerWriter.MaxDepth = value;
        }

        public override void WriteStartObject()
        {
            Palette.Object.Apply();
            InnerWriter.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            Palette.Object.Apply();
            InnerWriter.WriteEndObject();
        }

        public override void WriteMember(string name)
        {
            Palette.Member.Apply();
            InnerWriter.WriteMember(name);
        }

        public override void WriteStartArray()
        {
            Palette.Array.Apply();
            InnerWriter.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            Palette.Array.Apply();
            InnerWriter.WriteEndArray();
        }

        public override void WriteString(string value)
        {
            Palette.String.Apply();
            InnerWriter.WriteString(value);
        }

        public override void WriteNumber(string value)
        {
            Palette.Number.Apply();
            InnerWriter.WriteNumber(value);
        }

        public override void WriteBoolean(bool value)
        {
            Palette.Boolean.Apply();
            InnerWriter.WriteBoolean(value);
        }

        public override void WriteNull()
        {
            Palette.Null.Apply();
            InnerWriter.WriteNull();
        }

        public override void Flush()
        {
            InnerWriter.Flush();
        }

        public override void Close()
        {
            InnerWriter.Close();
        }
    }
}