using System;
using System.ComponentModel.Design;

namespace Redbox.Core
{
    internal class ServiceLocator : IServiceProvider
    {
        private ServiceContainer m_services;
        [ThreadStatic]
        private static ServiceContainer m_threadLocalServices;

        public static ServiceLocator Instance => Singleton<ServiceLocator>.Instance;

        public void AddService(Type serviceType, object instance)
        {
            this.Services.AddService(serviceType, instance);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            this.Services.AddService(serviceType, callback);
        }

        public void RemoveService(Type serviceType) => this.Services.RemoveService(serviceType);

        public void AddThreadLocalService(Type serviceType, object instance)
        {
            this.ThreadLocalServices.AddService(serviceType, instance);
        }

        public void AddThreadLocalService(Type serviceType, ServiceCreatorCallback callback)
        {
            this.ThreadLocalServices.AddService(serviceType, callback);
        }

        public void RemoveThreadLocalService(Type serviceType)
        {
            this.ThreadLocalServices.RemoveService(serviceType);
        }

        public T GetService<T>() => (T)this.GetService(typeof(T));

        public object GetService(Type serviceType)
        {
            return this.ThreadLocalServices.GetService(serviceType) ?? this.Services.GetService(serviceType);
        }

        internal IServiceContainer Services
        {
            get
            {
                if (this.m_services == null)
                    this.m_services = new ServiceContainer();
                return (IServiceContainer)this.m_services;
            }
        }

        internal IServiceContainer ThreadLocalServices
        {
            get
            {
                if (ServiceLocator.m_threadLocalServices == null)
                    ServiceLocator.m_threadLocalServices = new ServiceContainer();
                return (IServiceContainer)ServiceLocator.m_threadLocalServices;
            }
        }

        private ServiceLocator()
        {
        }
    }
}
