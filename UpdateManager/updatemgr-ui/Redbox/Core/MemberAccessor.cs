using System;
using System.Reflection;

namespace Redbox.Core
{
    internal class MemberAccessor
    {
        private readonly FieldInfo m_fieldInfo;
        private readonly MemberInfo m_memberInfo;
        private readonly PropertyInfo m_propertyInfo;

        public MemberAccessor(MemberInfo memberInfo)
        {
            this.m_memberInfo = memberInfo;
            if (this.m_memberInfo.MemberType == MemberTypes.Field)
                this.m_fieldInfo = (FieldInfo)this.m_memberInfo;
            else
                this.m_propertyInfo = this.m_memberInfo.MemberType == MemberTypes.Property ? (PropertyInfo)this.m_memberInfo : throw new ArgumentException("MemberAccessor only supports fields and properties.");
        }

        public Type Type
        {
            get
            {
                return !(this.m_fieldInfo != (FieldInfo)null) ? this.m_propertyInfo.PropertyType : this.m_fieldInfo.FieldType;
            }
        }

        public string Name => this.m_memberInfo.Name;

        public void SetValue(object instance, object value)
        {
            this.SetValue(instance, value, (object[])null);
        }

        public void SetValue(object instance, object value, object[] index)
        {
            if (this.m_fieldInfo != (FieldInfo)null)
                this.m_fieldInfo.SetValue(instance, value);
            else
                this.m_propertyInfo.SetValue(instance, value, index);
        }

        public object GetValue(object instance) => this.GetValue(instance, (object[])null);

        public object GetValue(object instance, object[] index)
        {
            return !(this.m_fieldInfo != (FieldInfo)null) ? this.m_propertyInfo.GetValue(instance, index) : this.m_fieldInfo.GetValue(instance);
        }
    }
}
