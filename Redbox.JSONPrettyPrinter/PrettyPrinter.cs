using System.IO;
using System.Text;
using Jayrock.Json;

namespace Redbox.JSONPrettyPrinter
{
    public static class PrettyPrinter
    {
        public static void Print(Stream i, Stream o)
        {
            using (var reader1 = new StreamReader(i))
            {
                using (var writer = new StreamWriter(o))
                {
                    using (var reader2 = new JsonTextReader(reader1))
                    {
                        using (var inner = new JsonTextWriter(writer))
                        {
                            inner.PrettyPrint = true;
                            new JsonColorWriter(inner, new JsonPalette()).WriteFromReader(reader2);
                            writer.WriteLine();
                        }
                    }
                }
            }
        }

        public static void Print(TextReader i, Stream o)
        {
            using (var writer = new StreamWriter(o))
            {
                using (var reader = new JsonTextReader(i))
                {
                    using (var inner = new JsonTextWriter(writer))
                    {
                        inner.PrettyPrint = true;
                        new JsonColorWriter(inner, new JsonPalette()).WriteFromReader(reader);
                        writer.WriteLine();
                    }
                }
            }
        }

        public static void Print(TextReader i, TextWriter o)
        {
            using (var reader = new JsonTextReader(i))
            {
                using (var inner = new JsonTextWriter(o))
                {
                    inner.PrettyPrint = true;
                    new JsonColorWriter(inner, new JsonPalette()).WriteFromReader(reader);
                    o.WriteLine();
                }
            }
        }

        public static string GetPrettyString(string input)
        {
            if (!input.StartsWith("{") && !input.EndsWith("}") && !input.StartsWith("[") && !input.EndsWith("]"))
                return input;
            var sb = new StringBuilder();
            using (var reader = new JsonTextReader(new StringReader(input)))
            {
                using (var jsonTextWriter = new JsonTextWriter(new StringWriter(sb)))
                {
                    jsonTextWriter.PrettyPrint = true;
                    jsonTextWriter.WriteFromReader(reader);
                }
            }

            return sb.ToString();
        }
    }
}