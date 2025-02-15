using System;
using System.Configuration;
using System.IO;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace Redbox.JSONPrettyPrinter
{
    [Serializable]
    internal sealed class JsonPalette : IJsonImportable
    {
        private ConsoleBrush arrayBrush;
        private ConsoleBrush booleanBrush;
        private ConsoleBrush defaultBrush;
        private ConsoleBrush memberBrush;
        private ConsoleBrush nullBrush;
        private ConsoleBrush numberBrush;
        private ConsoleBrush objectBrush;
        private ConsoleBrush stringBrush;

        public JsonPalette()
            : this(ConsoleBrush.Current)
        {
        }

        public JsonPalette(ConsoleBrush defaultBrush)
        {
            this.defaultBrush = defaultBrush;
            nullBrush = defaultBrush;
            stringBrush = defaultBrush;
            numberBrush = defaultBrush;
            booleanBrush = defaultBrush;
            objectBrush = defaultBrush;
            memberBrush = defaultBrush;
            arrayBrush = defaultBrush;
        }

        public ConsoleBrush DefaultBrush
        {
            get => defaultBrush;
            set => defaultBrush = value;
        }

        public ConsoleBrush Null
        {
            get => nullBrush;
            set => nullBrush = value;
        }

        public ConsoleBrush String
        {
            get => stringBrush;
            set => stringBrush = value;
        }

        public ConsoleBrush Number
        {
            get => numberBrush;
            set => numberBrush = value;
        }

        public ConsoleBrush Boolean
        {
            get => booleanBrush;
            set => booleanBrush = value;
        }

        public ConsoleBrush Object
        {
            get => objectBrush;
            set => objectBrush = value;
        }

        public ConsoleBrush Member
        {
            get => memberBrush;
            set => memberBrush = value;
        }

        public ConsoleBrush Array
        {
            get => arrayBrush;
            set => arrayBrush = value;
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
                    var lowerInvariant = reader.ReadMember().ToLowerInvariant();
                    var nullable = EnumHelper.TryParse<ConsoleColor>(reader.ReadString(), true);
                    ConsoleBrush consoleBrush;
                    int num;
                    if (!nullable.HasValue)
                    {
                        consoleBrush = DefaultBrush;
                        num = (int)consoleBrush.Foreground;
                    }
                    else
                    {
                        num = (int)nullable.GetValueOrDefault();
                    }

                    var consoleColor = (ConsoleColor)num;
                    if (lowerInvariant != null)
                    {
                        switch (lowerInvariant.Length)
                        {
                            case 3:
                                switch (lowerInvariant[0])
                                {
                                    case 'a':
                                        if (lowerInvariant == "arr")
                                            break;
                                        goto label_30;
                                    case 'b':
                                        if (lowerInvariant == "bit")
                                            goto label_28;
                                        goto label_30;
                                    case 'm':
                                        if (lowerInvariant == "mem")
                                            goto label_25;
                                        goto label_30;
                                    case 'n':
                                        switch (lowerInvariant)
                                        {
                                            case "num":
                                                goto label_27;
                                            case "nil":
                                                goto label_29;
                                            default:
                                                goto label_30;
                                        }
                                    case 'o':
                                        if (lowerInvariant == "obj")
                                            goto label_24;
                                        goto label_30;
                                    case 's':
                                        if (lowerInvariant == "str")
                                            goto label_26;
                                        goto label_30;
                                    default:
                                        goto label_30;
                                }

                                break;
                            case 4:
                                if (lowerInvariant == "null")
                                    goto label_29;
                                goto label_30;
                            case 5:
                                if (lowerInvariant == "array")
                                    break;
                                goto label_30;
                            case 6:
                                switch (lowerInvariant[0])
                                {
                                    case 'm':
                                        if (lowerInvariant == "member")
                                            goto label_25;
                                        goto label_30;
                                    case 'n':
                                        if (lowerInvariant == "number")
                                            goto label_27;
                                        goto label_30;
                                    case 'o':
                                        if (lowerInvariant == "object")
                                            goto label_24;
                                        goto label_30;
                                    case 's':
                                        if (lowerInvariant == "string")
                                            goto label_26;
                                        goto label_30;
                                    default:
                                        goto label_30;
                                }
                            case 7:
                                if (lowerInvariant == "boolean")
                                    goto label_28;
                                goto label_30;
                            default:
                                goto label_30;
                        }

                        consoleBrush = Array;
                        Array = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_24:
                        consoleBrush = Object;
                        Object = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_25:
                        consoleBrush = Member;
                        Member = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_26:
                        consoleBrush = String;
                        String = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_27:
                        consoleBrush = Number;
                        Number = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_28:
                        consoleBrush = Boolean;
                        Boolean = consoleBrush.ResetForeground(consoleColor);
                        goto label_30;
                        label_29:
                        consoleBrush = Null;
                        Null = consoleBrush.ResetForeground(consoleColor);
                    }

                    label_30: ;
                } while (reader.TokenClass != JsonTokenClass.EndObject);

                reader.Read();
            }
        }

        public static JsonPalette Auto()
        {
            return Auto(ConsoleBrush.Current);
        }

        public static JsonPalette Auto(ConsoleBrush defaultBrush)
        {
            string text;
            try
            {
                text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
            }
            catch (SettingsPropertyNotFoundException ex)
            {
                text = null;
            }

            if (string.IsNullOrEmpty(text))
            {
                text = "{nil=Gray,str=Yellow,num=Green,bit=Magenta,obj=Red,arr=Red,mem=Cyan}";
                defaultBrush = new ConsoleBrush(ConsoleColor.White, ConsoleColor.Black);
            }

            var jsonPalette = new JsonPalette(defaultBrush);
            jsonPalette.ImportJson(text);
            return jsonPalette;
        }

        public void ImportJson(string text)
        {
            ImportJson(new JsonTextReader(new StringReader(text)));
        }

        public void ImportJson(JsonReader reader)
        {
            ((IJsonImportable)this).Import(new ImportContext(), reader);
        }
    }
}