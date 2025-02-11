using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Redbox.Macros
{
    [Serializable]
    internal class PropertyDictionary : DictionaryBase
    {
        internal const string Visiting = "VISITING";
        internal const string Visited = "VISITED";
        private readonly StringCollection m_dynamicProperties = new StringCollection();
        private readonly StringCollection m_readOnlyProperties = new StringCollection();

        public virtual string this[string name]
        {
            get
            {
                string input = (string)this.Dictionary[(object)name];
                PropertyDictionary.CheckDeprecation(name);
                return !this.IsDynamicProperty(name) ? input : this.ExpandProperties(input, Location.UnknownLocation);
            }
            set => this.Dictionary[(object)name] = (object)value;
        }

        public virtual void AddReadOnly(string name, string value)
        {
            if (this.IsReadOnlyProperty(name))
                return;
            this.Dictionary.Add((object)name, (object)value);
            this.m_readOnlyProperties.Add(name);
        }

        public virtual void MarkDynamic(string name)
        {
            if (this.IsDynamicProperty(name))
                return;
            if (!this.Contains(name))
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1067")));
            this.m_dynamicProperties.Add(name);
        }

        public virtual void Add(string name, string value)
        {
            this.Dictionary.Add((object)name, (object)value);
        }

        public virtual bool IsReadOnlyProperty(string name) => this.m_readOnlyProperties.Contains(name);

        public virtual bool IsDynamicProperty(string name) => this.m_dynamicProperties.Contains(name);

        public virtual void Inherit(PropertyDictionary source, StringCollection excludes)
        {
            foreach (DictionaryEntry dictionaryEntry in source.Dictionary)
            {
                string key = (string)dictionaryEntry.Key;
                if ((excludes == null || !excludes.Contains(key)) && !this.IsReadOnlyProperty(key))
                {
                    PropertyDictionary.ValidatePropertyName(key, Location.UnknownLocation);
                    this.Dictionary[(object)key] = dictionaryEntry.Value;
                    if (source.IsReadOnlyProperty(key))
                        this.m_readOnlyProperties.Add(key);
                    if (source.IsDynamicProperty(key) && !this.IsDynamicProperty(key))
                        this.m_dynamicProperties.Add(key);
                }
            }
        }

        public string ExpandProperties(string input, Location location)
        {
            Hashtable state = new Hashtable();
            Stack visiting = new Stack();
            return this.ExpandProperties(input, location, state, visiting);
        }

        public bool Contains(string name) => this.Dictionary.Contains((object)name);

        public void Remove(string name) => this.Dictionary.Remove((object)name);

        public ICollection Keys => this.Dictionary.Keys;

        public ICollection Values => this.Dictionary.Values;

        public bool TreatUndefinedPropertyAsError { get; set; }

        internal string GetPropertyValue(string propertyName)
        {
            PropertyDictionary.CheckDeprecation(propertyName);
            return (string)this.Dictionary[(object)propertyName];
        }

        internal string ExpandProperties(
          string input,
          Location location,
          Hashtable state,
          Stack visiting)
        {
            return this.EvaluateEmbeddedExpressions(input, location, state, visiting);
        }

        protected override void OnClear()
        {
            this.m_readOnlyProperties.Clear();
            this.m_dynamicProperties.Clear();
        }

        protected override void OnSet(object key, object oldValue, object newValue)
        {
            string name = (string)key;
            if (this.IsReadOnlyProperty(name))
                throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1068"), (object)name), Location.UnknownLocation);
            base.OnSet(key, oldValue, newValue);
        }

        protected override void OnInsert(object key, object value)
        {
            string name = (string)key;
            if (this.Contains(name))
                throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1065"), (object)name), Location.UnknownLocation);
        }

        protected override void OnRemove(object key, object value)
        {
            if (!(key is string str) || !this.m_readOnlyProperties.Contains(str))
                return;
            this.m_readOnlyProperties.Remove(str);
        }

        protected override void OnValidate(object key, object value)
        {
            if (!(key is string propertyName))
                throw new ArgumentException("Property name must be a string.", nameof(key));
            PropertyDictionary.ValidatePropertyName(propertyName, Location.UnknownLocation);
            PropertyDictionary.ValidatePropertyValue(value, Location.UnknownLocation);
            base.OnValidate(key, value);
        }

        private string EvaluateEmbeddedExpressions(
          string input,
          Location location,
          Hashtable state,
          Stack visiting)
        {
            if (input == null)
                return (string)null;
            if (input.IndexOf('$') < 0)
                return input;
            try
            {
                StringBuilder stringBuilder = new StringBuilder(input.Length);
                ExpressionTokenizer tokenizer = new ExpressionTokenizer();
                ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator(this, state, visiting);
                tokenizer.IgnoreWhitespace = false;
                tokenizer.SingleCharacterMode = true;
                tokenizer.InitTokenizer(input);
                while (tokenizer.CurrentToken != ExpressionTokenizer.TokenType.EOF)
                {
                    if (tokenizer.CurrentToken == ExpressionTokenizer.TokenType.Dollar)
                    {
                        tokenizer.GetNextToken();
                        if (tokenizer.CurrentToken == ExpressionTokenizer.TokenType.LeftCurlyBrace)
                        {
                            tokenizer.IgnoreWhitespace = true;
                            tokenizer.SingleCharacterMode = false;
                            tokenizer.GetNextToken();
                            string str = Convert.ToString(expressionEvaluator.Evaluate(tokenizer), (IFormatProvider)CultureInfo.InvariantCulture);
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
                }
                return stringBuilder.ToString();
            }
            catch (ExpressionParseException ex)
            {
                StringBuilder stringBuilder = new StringBuilder();
                string str = input.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
                stringBuilder.Append(ex.Message);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("Expression: ");
                stringBuilder.Append(str);
                int startPos = ex.StartPos;
                int num = ex.EndPos;
                if (startPos != -1 || num != -1)
                {
                    stringBuilder.Append(Environment.NewLine);
                    if (num == -1)
                        num = startPos + 1;
                    for (int index = 0; index < startPos + "Expression: ".Length; ++index)
                        stringBuilder.Append(' ');
                    for (int index = startPos; index < num; ++index)
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
                throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), (object)propertyName), location);
            if (propertyName.EndsWith("-") || propertyName.EndsWith("."))
                throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1064"), (object)propertyName), location);
        }

        private static void ValidatePropertyValue(object value, Location location)
        {
            if (value != null && !(value is string))
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1066"), (object)value.GetType()), nameof(value));
        }

        internal static EvaluatorException CreateCircularException(string end, Stack stack)
        {
            StringBuilder stringBuilder = new StringBuilder("Circular property reference: ");
            stringBuilder.Append(end);
            string str;
            do
            {
                str = (string)stack.Pop();
                stringBuilder.Append(" <- ");
                stringBuilder.Append(str);
            }
            while (!str.Equals(end));
            return new EvaluatorException(stringBuilder.ToString());
        }
    }
}
