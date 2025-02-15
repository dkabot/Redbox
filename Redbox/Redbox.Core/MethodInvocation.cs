using System.Reflection;

namespace Redbox.Core
{
    public class MethodInvocation
    {
        private readonly MethodInfo m_method;
        private readonly object[] m_parameters;
        private readonly object m_target;

        public MethodInvocation(object target, MethodInfo method, params object[] parms)
        {
            m_target = target;
            m_method = method;
            m_parameters = parms;
        }

        public object Invoke()
        {
            return !(m_method == null) ? m_method.Invoke(m_target, m_parameters) : null;
        }
    }
}