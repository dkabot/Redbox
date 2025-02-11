using Redbox.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Redbox.GetOpts
{
    internal class OptionValue
    {
        private readonly object m_container;
        private readonly MemberAccessor m_member;
        private readonly OptionAttribute m_definition;
        private readonly DescriptionAttribute m_description;
        private readonly DefaultValueAttribute m_defaultValue;

        public static OptionValueList GetOptionValues(object container)
        {
            OptionValueList optionValues = new OptionValueList();
            foreach (MemberInfo member in container.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                OptionAttribute customAttribute1 = (OptionAttribute)Attribute.GetCustomAttribute(member, typeof(OptionAttribute));
                if (customAttribute1 != null)
                {
                    DescriptionAttribute customAttribute2 = (DescriptionAttribute)Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute));
                    DefaultValueAttribute customAttribute3 = (DefaultValueAttribute)Attribute.GetCustomAttribute(member, typeof(DefaultValueAttribute));
                    optionValues.AddLast(new OptionValue(container, new MemberAccessor(member), customAttribute1, customAttribute2, customAttribute3));
                }
            }
            return optionValues;
        }

        public void SetValue(object key, object value)
        {
            if (this.IsDictionary())
            {
                PropertyInfo property = this.m_member.Type.GetProperty("Item");
                Type genericArgument1 = this.m_member.Type.GetGenericArguments()[0];
                Type genericArgument2 = this.m_member.Type.GetGenericArguments()[1];
                object obj1 = this.m_member.GetValue(this.m_container);
                object obj2 = ConversionHelper.ChangeType(value, genericArgument2);
                object[] index = new object[1]
                {
          ConversionHelper.ChangeType(key, genericArgument1)
                };
                property.SetValue(obj1, obj2, index);
            }
            else if (this.IsList())
            {
                Type genericArgument = this.m_member.Type.GetGenericArguments()[0];
                this.m_member.Type.GetMethod("Insert").Invoke(this.m_member.GetValue(this.m_container), new object[2]
                {
          (object) 0,
          ConversionHelper.ChangeType(value, genericArgument)
                });
            }
            else
            {
                object obj = ConversionHelper.ChangeType(value, this.m_member.Type);
                this.EnsureConstraints(obj);
                this.m_member.SetValue(this.m_container, obj);
            }
        }

        public object GetValue() => this.m_member.GetValue(this.m_container);

        public override string ToString()
        {
            int num1 = 80;
            try
            {
                num1 = Console.WindowWidth;
            }
            catch
            {
            }
            StringBuilder stringBuilder = new StringBuilder();
            string str1 = string.Empty;
            if (this.m_definition.LongName != null)
                str1 = string.Format("--{0}", (object)this.m_definition.LongName);
            if (this.m_definition.ShortName != null)
            {
                if (this.m_definition.LongName != null)
                    str1 += ", ";
                str1 += string.Format("-{0}", (object)this.m_definition.ShortName);
            }
            if (this.m_definition.LongName == null && this.m_definition.ShortName == null)
                str1 += string.Format("--{0}", (object)this.m_member.Name.ToLowerInvariant());
            if (this.IsFlag())
                str1 += "[+|-]";
            else if (this.IsDictionary())
                str1 += ":key=value";
            stringBuilder.AppendFormat("  {0,-15}", (object)str1);
            int num2 = 15;
            if (this.m_definition.Required)
            {
                stringBuilder.Append("Required. ");
                num2 += 10;
            }
            if (this.m_defaultValue != null)
            {
                string str2 = string.Format("Default value is '{0}'. ", this.m_defaultValue.Value);
                num2 += str2.Length;
                stringBuilder.Append(str2);
            }
            if (this.m_description != null)
            {
                foreach (string str3 in this.m_description.Description.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    stringBuilder.AppendFormat("{0} ", (object)str3);
                    num2 += str3.Length + 1;
                    if (num2 > num1 - 15)
                    {
                        stringBuilder.Append("\n                 ");
                        num2 = 15;
                    }
                }
            }
            if (this.m_definition.Min != null && this.m_definition.Max != null)
                stringBuilder.AppendFormat("\n                  Valid values range from {0} to {1} inclusive.", this.m_definition.Min, this.m_definition.Max);
            return stringBuilder.ToString();
        }

        public string GetName()
        {
            return this.m_definition.LongName ?? this.m_definition.ShortName ?? this.m_member.Name;
        }

        public bool IsValidName(string name)
        {
            return string.Compare(name, this.m_definition.LongName, true) == 0 || string.Compare(name, this.m_definition.ShortName, true) == 0 || string.Compare(name, this.m_member.Name, true) == 0;
        }

        public bool Required => this.m_definition.Required;

        internal OptionValue(
          object container,
          MemberAccessor member,
          OptionAttribute definition,
          DescriptionAttribute description,
          DefaultValueAttribute defaultValue)
        {
            this.m_member = member;
            this.m_container = container;
            this.m_definition = definition;
            this.m_description = description;
            this.m_defaultValue = defaultValue;
            if (this.m_defaultValue == null)
                return;
            this.SetValue((object)null, this.m_defaultValue.Value);
        }

        internal void EnsureConstraints(object value)
        {
            if (this.m_definition.Min != null && this.m_definition.Max != null && value is IComparable comparable && (comparable.CompareTo(this.m_definition.Min) < 0 || comparable.CompareTo(this.m_definition.Max) > 0))
                throw new ArgumentOutOfRangeException(string.Format("{0} must be a value from {1} to {2}.", (object)this.GetName(), this.m_definition.Min, this.m_definition.Max));
        }

        internal bool IsFlag() => this.m_member.Type == typeof(bool);

        internal bool IsList()
        {
            return this.m_member.Type.IsGenericType && this.m_member.Type.GetGenericTypeDefinition() == typeof(IList<>);
        }

        internal bool IsDictionary()
        {
            return this.m_member.Type.IsGenericType && this.m_member.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }
    }
}
