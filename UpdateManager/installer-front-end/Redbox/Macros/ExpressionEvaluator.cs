using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Redbox.Macros
{
    [FunctionSet("property", "NAnt")]
    internal class ExpressionEvaluator : ExpressionEvalBase
    {
        private readonly Stack m_visiting;
        private readonly Hashtable m_state;
        private readonly PropertyDictionary m_properties;

        public ExpressionEvaluator(PropertyDictionary properties, Hashtable state, Stack visiting)
        {
            this.m_properties = properties;
            this.m_state = state;
            this.m_visiting = visiting;
        }

        [Function("get-value")]
        public string GetPropertyValue(string propertyName)
        {
            if (this.m_properties.IsDynamicProperty(propertyName))
            {
                if ((string)this.m_state[(object)propertyName] == "VISITING")
                    throw PropertyDictionary.CreateCircularException(propertyName, this.m_visiting);
                this.m_visiting.Push((object)propertyName);
                this.m_state[(object)propertyName] = (object)"VISITING";
                string propertyValue1 = this.m_properties.GetPropertyValue(propertyName);
                if (propertyValue1 == null)
                    throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1053"), (object)propertyName));
                Location unknownLocation = Location.UnknownLocation;
                string propertyValue2 = this.m_properties.ExpandProperties(propertyValue1, unknownLocation, this.m_state, this.m_visiting);
                this.m_visiting.Pop();
                this.m_state[(object)propertyName] = (object)"VISITED";
                return propertyValue2;
            }
            string propertyValue = this.m_properties.GetPropertyValue(propertyName);
            if (propertyValue == null)
            {
                if (this.m_properties.TreatUndefinedPropertyAsError)
                    throw new EvaluatorException(string.Format((IFormatProvider)CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1053"), (object)propertyName));
                propertyValue = string.Empty;
            }
            return propertyValue;
        }

        protected override object EvaluateProperty(string propertyName)
        {
            return (object)this.GetPropertyValue(propertyName);
        }

        protected override object EvaluateFunction(MethodInfo methodInfo, object[] args)
        {
            try
            {
                if (methodInfo.IsStatic)
                    return methodInfo.Invoke((object)null, args);
                if (methodInfo.DeclaringType.IsAssignableFrom(typeof(ExpressionEvaluator)))
                    return methodInfo.Invoke((object)this, args);
                object obj = methodInfo.DeclaringType.GetConstructor(new Type[1]
                {
          typeof (PropertyDictionary)
                }).Invoke(new object[1]
                {
          (object) this.m_properties
                });
                return methodInfo.Invoke(obj, args);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }
    }
}
