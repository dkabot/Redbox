using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.GetOpts
{
    public class OptionValue
    {
        private readonly object m_container;
        private readonly DefaultValueAttribute m_defaultValue;
        private readonly OptionAttribute m_definition;
        private readonly DescriptionAttribute m_description;
        private readonly MemberAccessor m_member;

        internal OptionValue(
            object container,
            MemberAccessor member,
            OptionAttribute definition,
            DescriptionAttribute description,
            DefaultValueAttribute defaultValue)
        {
            m_member = member;
            m_container = container;
            m_definition = definition;
            m_description = description;
            m_defaultValue = defaultValue;
            if (m_defaultValue == null)
                return;
            SetValue(null, m_defaultValue.Value);
        }

        public bool Required => m_definition.Required;

        public static OptionValueList GetOptionValues(object container)
        {
            var optionValues = new OptionValueList();
            foreach (var member in container.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                                  BindingFlags.GetField | BindingFlags.SetField |
                                                                  BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                var customAttribute1 = (OptionAttribute)Attribute.GetCustomAttribute(member, typeof(OptionAttribute));
                if (customAttribute1 != null)
                {
                    var customAttribute2 =
                        (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute));
                    var customAttribute3 =
                        (DefaultValueAttribute)Attribute.GetCustomAttribute(member, typeof(DefaultValueAttribute));
                    optionValues.AddLast(new OptionValue(container, new MemberAccessor(member), customAttribute1,
                        customAttribute2, customAttribute3));
                }
            }

            return optionValues;
        }

        public void SetValue(object key, object value)
        {
            if (IsDictionary())
            {
                var property = m_member.Type.GetProperty("Item");
                var genericArgument1 = m_member.Type.GetGenericArguments()[0];
                var genericArgument2 = m_member.Type.GetGenericArguments()[1];
                var obj1 = m_member.GetValue(m_container);
                var obj2 = ConversionHelper.ChangeType(value, genericArgument2);
                var index = new object[1]
                {
                    ConversionHelper.ChangeType(key, genericArgument1)
                };
                property.SetValue(obj1, obj2, index);
            }
            else if (IsList())
            {
                var genericArgument = m_member.Type.GetGenericArguments()[0];
                m_member.Type.GetMethod("Insert").Invoke(m_member.GetValue(m_container), new object[2]
                {
                    0,
                    ConversionHelper.ChangeType(value, genericArgument)
                });
            }
            else
            {
                var obj = ConversionHelper.ChangeType(value, m_member.Type);
                EnsureConstraints(obj);
                m_member.SetValue(m_container, obj);
            }
        }

        public object GetValue()
        {
            return m_member.GetValue(m_container);
        }

        public override string ToString()
        {
            var num1 = 80;
            try
            {
                num1 = Console.WindowWidth;
            }
            catch
            {
            }

            var stringBuilder = new StringBuilder();
            var str1 = string.Empty;
            if (m_definition.LongName != null)
                str1 = string.Format("--{0}", m_definition.LongName);
            if (m_definition.ShortName != null)
            {
                if (m_definition.LongName != null)
                    str1 += ", ";
                str1 += string.Format("-{0}", m_definition.ShortName);
            }

            if (m_definition.LongName == null && m_definition.ShortName == null)
                str1 += string.Format("--{0}", m_member.Name.ToLowerInvariant());
            if (IsFlag())
                str1 += "[+|-]";
            else if (IsDictionary())
                str1 += ":key=value";
            stringBuilder.AppendFormat("  {0,-15}", str1);
            var num2 = 15;
            if (m_definition.Required)
            {
                stringBuilder.Append("Required. ");
                num2 += 10;
            }

            if (m_defaultValue != null)
            {
                var str2 = string.Format("Default value is '{0}'. ", m_defaultValue.Value);
                num2 += str2.Length;
                stringBuilder.Append(str2);
            }

            if (m_description != null)
                foreach (var str3 in m_description.Description.Split(" ".ToCharArray(),
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    stringBuilder.AppendFormat("{0} ", str3);
                    num2 += str3.Length + 1;
                    if (num2 > num1 - 15)
                    {
                        stringBuilder.Append("\n                 ");
                        num2 = 15;
                    }
                }

            if (m_definition.Min != null && m_definition.Max != null)
                stringBuilder.AppendFormat("\n                  Valid values range from {0} to {1} inclusive.",
                    m_definition.Min, m_definition.Max);
            return stringBuilder.ToString();
        }

        public string GetName()
        {
            return m_definition.LongName ?? m_definition.ShortName ?? m_member.Name;
        }

        public bool IsValidName(string name)
        {
            return string.Compare(name, m_definition.LongName, true) == 0 ||
                   string.Compare(name, m_definition.ShortName, true) == 0 ||
                   string.Compare(name, m_member.Name, true) == 0;
        }

        internal void EnsureConstraints(object value)
        {
            if (m_definition.Min != null && m_definition.Max != null && value is IComparable comparable &&
                (comparable.CompareTo(m_definition.Min) < 0 || comparable.CompareTo(m_definition.Max) > 0))
                throw new ArgumentOutOfRangeException(string.Format("{0} must be a value from {1} to {2}.", GetName(),
                    m_definition.Min, m_definition.Max));
        }

        internal bool IsFlag()
        {
            return m_member.Type == typeof(bool);
        }

        internal bool IsList()
        {
            return m_member.Type.IsGenericType && m_member.Type.GetGenericTypeDefinition() == typeof(IList<>);
        }

        internal bool IsDictionary()
        {
            return m_member.Type.IsGenericType && m_member.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }
    }
}