using Jayrock.Json;
using System.IO;
using System.Text;

namespace Redbox.JSONPrettyPrinter
{
    internal static class PrettyPrinter
    {
        public static void Print(Stream i, Stream o)
        {
            using (StreamReader reader1 = new StreamReader(i))
            {
                using (StreamWriter writer = new StreamWriter(o))
                {
                    using (JsonTextReader reader2 = new JsonTextReader((TextReader)reader1))
                    {
                        using (JsonTextWriter inner = new JsonTextWriter((TextWriter)writer))
                        {
                            inner.PrettyPrint = true;
                            new JsonColorWriter((JsonWriter)inner, new JsonPalette()).WriteFromReader((JsonReader)reader2);
                            writer.WriteLine();
                        }
                    }
                }
            }
        }

        public static void Print(TextReader i, Stream o)
        {
            using (StreamWriter writer = new StreamWriter(o))
            {
                using (JsonTextReader reader = new JsonTextReader(i))
                {
                    using (JsonTextWriter inner = new JsonTextWriter((TextWriter)writer))
                    {
                        inner.PrettyPrint = true;
                        new JsonColorWriter((JsonWriter)inner, new JsonPalette()).WriteFromReader((JsonReader)reader);
                        writer.WriteLine();
                    }
                }
            }
        }

        public static void Print(TextReader i, TextWriter o)
        {
            using (JsonTextReader reader = new JsonTextReader(i))
            {
                using (JsonTextWriter inner = new JsonTextWriter(o))
                {
                    inner.PrettyPrint = true;
                    new JsonColorWriter((JsonWriter)inner, new JsonPalette()).WriteFromReader((JsonReader)reader);
                    o.WriteLine();
                }
            }
        }

        public static string GetPrettyString(string input)
        {
            if (!input.StartsWith("{") && !input.EndsWith("}") && !input.StartsWith("[") && !input.EndsWith("]"))
                return input;
            StringBuilder sb = new StringBuilder();
            using (JsonTextReader reader = new JsonTextReader((TextReader)new StringReader(input)))
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter)new StringWriter(sb)))
                {
                    jsonTextWriter.PrettyPrint = true;
                    jsonTextWriter.WriteFromReader((JsonReader)reader);
                }
            }
            return sb.ToString();
        }
    }
}
