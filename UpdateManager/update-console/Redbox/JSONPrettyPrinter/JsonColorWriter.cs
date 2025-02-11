using Jayrock.Json;

namespace Redbox.JSONPrettyPrinter
{
    internal sealed class JsonColorWriter : JsonWriter
    {
        private readonly JsonWriter inner;
        private JsonPalette palette;

        public JsonColorWriter(JsonWriter inner)
          : this(inner, (JsonPalette)null)
        {
        }

        public JsonColorWriter(JsonWriter inner, JsonPalette palette)
        {
            this.inner = inner;
            this.palette = palette ?? JsonPalette.Auto();
        }

        public JsonWriter InnerWriter => this.inner;

        public JsonPalette Palette
        {
            get => this.palette;
            set => this.palette = value;
        }

        public override int Index => this.inner.Index;

        public override JsonWriterBracket Bracket => this.inner.Bracket;

        public override void WriteStartObject()
        {
            this.Palette.Object.Apply();
            this.inner.WriteStartObject();
        }

        public override void WriteEndObject()
        {
            this.Palette.Object.Apply();
            this.inner.WriteEndObject();
        }

        public override void WriteMember(string name)
        {
            this.Palette.Member.Apply();
            this.inner.WriteMember(name);
        }

        public override void WriteStartArray()
        {
            this.Palette.Array.Apply();
            this.inner.WriteStartArray();
        }

        public override void WriteEndArray()
        {
            this.Palette.Array.Apply();
            this.inner.WriteEndArray();
        }

        public override void WriteString(string value)
        {
            this.Palette.String.Apply();
            this.inner.WriteString(value);
        }

        public override void WriteNumber(string value)
        {
            this.Palette.Number.Apply();
            this.inner.WriteNumber(value);
        }

        public override void WriteBoolean(bool value)
        {
            this.Palette.Boolean.Apply();
            this.inner.WriteBoolean(value);
        }

        public override void WriteNull()
        {
            this.Palette.Null.Apply();
            this.inner.WriteNull();
        }

        public override void Flush() => this.inner.Flush();

        public override void Close() => this.inner.Close();

        public override int Depth => this.inner.Depth;

        public override int MaxDepth
        {
            get
            {
                return this.inner.MaxDepth;
            }

            set
            {
                this.inner.MaxDepth = value;
            }
        }


    }
}
