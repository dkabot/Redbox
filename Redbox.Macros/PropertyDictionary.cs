using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Macros
{
    [Serializable]
    public class PropertyDictionary : DictionaryBase
    {
        internal const string Visiting = "VISITING";
        internal const string Visited = "VISITED";
        private readonly StringCollection m_dynamicProperties = new StringCollection();
        private readonly StringCollection m_readOnlyProperties = new StringCollection();

        public virtual string this[string name]
        {
            get
            {
                var input = (string)Dictionary[name];
                CheckDeprecation(name);
                return !IsDynamicProperty(name) ? input : ExpandProperties(input, Location.UnknownLocation);
            }
            set => Dictionary[name] = value;
        }

        public ICollection Keys => Dictionary.Keys;

        public ICollection Values => Dictionary.Values;

        public bool TreatUndefinedPropertyAsError { get; set; }

        public virtual void AddReadOnly(string name, string value)
        {
            if (IsReadOnlyProperty(name))
                return;
            Dictionary.Add(name, value);
            m_readOnlyProperties.Add(name);
        }

        public virtual void MarkDynamic(string name)
        {
            if (IsDynamicProperty(name))
                return;
            if (!Contains(name))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    ResourceUtils.GetString("NA1067")));
            m_dynamicProperties.Add(name);
        }

        public virtual void Add(string name, string value)
        {
            Dictionary.Add(name, value);
        }

        public virtual bool IsReadOnlyProperty(string name)
        {
            return m_readOnlyProperties.Contains(name);
        }

        public virtual bool IsDynamicProperty(string name)
        {
            return m_dynamicProperties.Contains(name);
        }

        public virtual void Inherit(PropertyDictionary source, StringCollection excludes)
        {
            foreach (DictionaryEntry dictionaryEntry in source.Dictionary)
            {
                var key = (string)dictionaryEntry.Key;
                if ((excludes == null || !excludes.Contains(key)) && !IsReadOnlyProperty(key))
                {
                    ValidatePropertyName(key, Location.UnknownLocation);
                    Dictionary[key] = dictionaryEntry.Value;
                    if (source.IsReadOnlyProperty(key))
                        m_readOnlyProperties.Add(key);
                    if (source.IsDynamicProperty(key) && !IsDynamicProperty(key))
                        m_dynamicProperties.Add(key);
                }
            }
        }

        public string ExpandProperties(string input, Location location)
        {
            var state = new Hashtable();
            var visiting = new Stack();
            return ExpandProperties(input, location, state, visiting);
        }

        public bool Contains(string name)
        {
            return Dictionary.Contains(name);
        }

        public void Remove(string name)
        {
            Dictionary.Remove(name);
        }

        internal string GetPropertyValue(string propertyName)
        {
            CheckDeprecation(propertyName);
            return (string)Dictionary[propertyName];
        }

        internal string ExpandProperties(
            string input,
            Location location,
            Hashtable state,
            Stack visiting)
        {
            return EvaluateEmbeddedExpressions(input, location, state, visiting);
        }

        protected override void OnClear()
        {
            m_readOnlyProperties.Clear();
            m_dynamicProperties.Clear();
        }

        protected override void OnSet(object key, object oldValue, object newValue)
        {
            var name = (string)key;
            if (IsReadOnlyProperty(name))
                throw new EvaluatorException(
                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1068"), name),
                    Location.UnknownLocation);
            base.OnSet(key, oldValue, newValue);
        }

        protected override void OnInsert(object key, object value)
        {
            var name = (string)key;
            if (Contains(name))
                throw new EvaluatorException(
                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1065"), name),
                    Location.UnknownLocation);
        }

        protected override void OnRemove(object key, object value)
        {
            if (!(key is string str) || !m_readOnlyProperties.Contains(str))
                return;
            m_readOnlyProperties.Remove(str);
        }

        protected override void OnValidate(object key, object value)
        {
            if (!(key is string propertyName))
                throw new ArgumentException("Property name must be a string.", nameof(key));
            ValidatePropertyName(propertyName, Location.UnknownLocation);
            ValidatePropertyValue(value, Location.UnknownLocation);
            base.OnValidate(key, value);
        }

        private string EvaluateEmbeddedExpressions(
            string input,
            Location location,
            Hashtable state,
            Stack visiting)
        {
            if (input == null)
                return null;
            if (input.IndexOf('$') < 0)
                return input;
            try
            {
                var stringBuilder = new StringBuilder(input.Length);
                var tokenizer = new ExpressionTokenizer();
                var expressionEvaluator = new ExpressionEvaluator(this, state, visiting);
                tokenizer.IgnoreWhitespace = false;
                tokenizer.SingleCharacterMode = true;
                tokenizer.InitTokenizer(input);
                while (tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                    if (tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dollar)
                    {
                        tokenizer.GetNextToken();
                        if (tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftCurlyBrace)
                        {
                            tokenizer.IgnoreWhitespace = true;
                            tokenizer.SingleCharacterMode = false;
                            tokenizer.GetNextToken();
                            var str = Convert.ToString(expressionEvaluator.Evaluate(tokenizer),
                                CultureInfo.InvariantCulture);
                            stringBuilder.Append(str);
                            tokenizer.IgnoreWhitespace = false;
                            if (tokenizer.CurrentToken != ExpressionTokenizer.TokenType.RightCurlyBrace)
                                throw new ExpressionParseException("'}' expected", tokenizer.CurrentPosition.CharIndex);
                            tokenizer.SingleCharacterMode = true;
                            tokenizer.GetNextToken();
                        }
                        else
                        {
                            stringBuilder.Append('$');
                            if (tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                            {
                                stringBuilder.Append(tokenizer.TokenText);
                                tokenizer.GetNextToken();
                            }
                        }
                    }
                    else
                    {
                        stringBuilder.Append(tokenizer.TokenText);
                        tokenizer.GetNextToken();
                    }

                return stringBuilder.ToString();
            }
            catch (ExpressionParseException ex)
            {
                var stringBuilder = new StringBuilder();
                var str = input.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
                stringBuilder.Append(ex.Message);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("Expression: ");
                stringBuilder.Append(str);
                var startPos = ex.StartPos;
                var num = ex.EndPos;
                if (startPos != -1 || num != -1)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (num == -1)
                        num = startPos + 1;
                    for (var index = 0; index < startPos + "Expression: ".Length; ++index)
                        stringBuilder.Append(' ');
                    for (var index = startPos; index < num; ++index)
                        stringBuilder.Append('^');
                }

                throw new EvaluatorException(stringBuilder.ToString(), location, ex.InnerException);
            }
        }

        private static void CheckDeprecation(string name)
        {
        }

        private static void ValidatePropertyName(string propertyName, Location location)
        {
            if (!Regex.IsMatch(propertyName, "^[_A-Za-z0-9][_A-Za-z0-9\\-:.()]*$"))
                throw new EvaluatorException(
                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), propertyName),
                    location);
            if (propertyName.EndsWith("-") || propertyName.EndsWith("."))
                throw new EvaluatorException(
                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), propertyName),
                    location);
        }

        private static void ValidatePropertyValue(object value, Location location)
        {
            if (value != null && !(value is string))
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1066"), value.GetType()),
                    nameof(value));
        }

        internal static EvaluatorException CreateCircularException(string end, Stack stack)
        {
            var stringBuilder = new StringBuilder("Circular property reference: ");
            stringBuilder.Append(end);
            string str;
            do
            {
                str = (string)stack.Pop();
                stringBuilder.Append(" <- ");
                stringBuilder.Append(str);
            } while (!str.Equals(end));

            return new EvaluatorException(stringBuilder.ToString());
        }
    }
}