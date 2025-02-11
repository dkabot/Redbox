using Jayrock.Json;
using Jayrock.Json.Conversion;
using System;
using System.Configuration;
using System.IO;

namespace Redbox.JSONPrettyPrinter
{
    [Serializable]
    internal sealed class JsonPalette : IJsonImportable
    {
        private ConsoleBrush defaultBrush;
        private ConsoleBrush nullBrush;
        private ConsoleBrush stringBrush;
        private ConsoleBrush numberBrush;
        private ConsoleBrush booleanBrush;
        private ConsoleBrush objectBrush;
        private ConsoleBrush memberBrush;
        private ConsoleBrush arrayBrush;

        public JsonPalette()
          : this(ConsoleBrush.Current)
        {
        }

        public JsonPalette(ConsoleBrush defaultBrush)
        {
            this.defaultBrush = defaultBrush;
            this.nullBrush = defaultBrush;
            this.stringBrush = defaultBrush;
            this.numberBrush = defaultBrush;
            this.booleanBrush = defaultBrush;
            this.objectBrush = defaultBrush;
            this.memberBrush = defaultBrush;
            this.arrayBrush = defaultBrush;
        }

        public static JsonPalette Auto() => JsonPalette.Auto(ConsoleBrush.Current);

        public static JsonPalette Auto(ConsoleBrush defaultBrush)
        {
            string text;
            try
            {
                text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
            }
            catch (SettingsPropertyNotFoundException ex)
            {
                text = (string)null;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
                defaultBrush = new ConsoleBrush(ConsoleColor.White, ConsoleColor.Black);
            }
            JsonPalette jsonPalette = new JsonPalette(defaultBrush);
            jsonPalette.ImportJson(text);
            return jsonPalette;
        }

        public ConsoleBrush DefaultBrush
        {
            get => this.defaultBrush;
            set => this.defaultBrush = value;
        }

        public ConsoleBrush Null
        {
            get => this.nullBrush;
            set => this.nullBrush = value;
        }

        public ConsoleBrush String
        {
            get => this.stringBrush;
            set => this.stringBrush = value;
        }

        public ConsoleBrush Number
        {
            get => this.numberBrush;
            set => this.numberBrush = value;
        }

        public ConsoleBrush Boolean
        {
            get => this.booleanBrush;
            set => this.booleanBrush = value;
        }

        public ConsoleBrush Object
        {
            get => this.objectBrush;
            set => this.objectBrush = value;
        }

        public ConsoleBrush Member
        {
            get => this.memberBrush;
            set => this.memberBrush = value;
        }

        public ConsoleBrush Array
        {
            get => this.arrayBrush;
            set => this.arrayBrush = value;
        }

        public void ImportJson(string text)
        {
            this.ImportJson((JsonReader)new JsonTextReader((TextReader)new StringReader(text)));
        }

        public void ImportJson(JsonReader reader)
        {
            ((IJsonImportable)this).Import(new ImportContext(), reader);
        }

        void IJsonImportable.Import(ImportContext context, JsonReader reader)
        {
            reader.MoveToContent();
            if (reader.TokenClass != JsonTokenClass.Object)
            {
                reader.Skip();
            }
            else
            {
                reader.Read();
                do
                {
                    string lowerInvariant = reader.ReadMember().ToLowerInvariant();
                    ConsoleColor? nullable = EnumHelper.TryParse<ConsoleColor>(reader.ReadString(), true);
                    ConsoleBrush consoleBrush;
                    int num;
                    if (!nullable.HasValue)
                    {
                        consoleBrush = this.DefaultBrush;
                        num = (int)consoleBrush.Foreground;
                    }
                    else
                        num = (int)nullable.GetValueOrDefault();
                    ConsoleColor consoleColor = (ConsoleColor)num;
                    switch (lowerInvariant)
                    {
                        case "arr":
                        case "array":
                            consoleBrush = this.Array;
                            this.Array = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "bit":
                        case "boolean":
                            consoleBrush = this.Boolean;
                            this.Boolean = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "mem":
                        case "member":
                            consoleBrush = this.Member;
                            this.Member = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "nil":
                        case "null":
                            consoleBrush = this.Null;
                            this.Null = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "num":
                        case "number":
                            consoleBrush = this.Number;
                            this.Number = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "obj":
                        case "object":
                            consoleBrush = this.Object;
                            this.Object = consoleBrush.ResetForeground(consoleColor);
                            break;
                        case "str":
                        case "string":
                            consoleBrush = this.String;
                            this.String = consoleBrush.ResetForeground(consoleColor);
                            break;
                    }
                }
                while (reader.TokenClass != JsonTokenClass.EndObject);
                reader.Read();
            }
        }
    }
}
