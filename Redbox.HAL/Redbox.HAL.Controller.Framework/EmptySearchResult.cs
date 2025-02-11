using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class EmptySearchResult : IEmptySearchResult, IDisposable
    {
        private bool m_disposed;

        internal EmptySearchResult()
        {
            EmptyLocations = new List<ILocation>();
        }

        public void Dispose()
        {
            if (m_disposed)
                return;
            m_disposed = true;
            EmptyLocations.Clear();
            GC.SuppressFinalize(this);
        }

        public IList<ILocation> EmptyLocations { get; }

        public int FoundEmpty => EmptyLocations.Count;
    }
}