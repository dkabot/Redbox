using System.Reflection;

namespace Redbox.Core
{
    internal class MethodInvocation
    {
        private readonly object m_target;
        private readonly MethodInfo m_method;
        private readonly object[] m_parameters;

        public MethodInvocation(object target, MethodInfo method, params object[] parms)
        {
            this.m_target = target;
            this.m_method = method;
            this.m_parameters = parms;
        }

        public object Invoke()
        {
            return !(this.m_method == (MethodInfo)null) ? this.m_method.Invoke(this.m_target, this.m_parameters) : (object)null;
        }
    }
}
