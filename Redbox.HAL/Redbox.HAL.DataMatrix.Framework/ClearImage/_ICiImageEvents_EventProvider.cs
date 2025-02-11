using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace ClearImage
{
    public class _ICiImageEvents_EventProvider : _ICiImageEvents_Event, IDisposable
    {
        private readonly IConnectionPointContainer m_ConnectionPointContainer;
        private ArrayList m_aEventSinkHelpers;
        private IConnectionPoint m_ConnectionPoint;

        public _ICiImageEvents_EventProvider(object object_0)
        {
            m_ConnectionPointContainer = (IConnectionPointContainer)object_0;
        }

        public virtual event _ICiImageEvents_LogEventHandler Log
        {
            add
            {
                Monitor.Enter(this);
                try
                {
                    if (m_ConnectionPoint == null)
                        Init();
                    var pUnkSink = new _ICiImageEvents_SinkHelper();
                    var pdwCookie = 0;
                    m_ConnectionPoint.Advise(pUnkSink, out pdwCookie);
                    pUnkSink.m_dwCookie = pdwCookie;
                    pUnkSink.m_LogDelegate = value;
                    m_aEventSinkHelpers.Add(pUnkSink);
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            remove
            {
                Monitor.Enter(this);
                try
                {
                    if (m_aEventSinkHelpers == null)
                        return;
                    var count = m_aEventSinkHelpers.Count;
                    var index = 0;
                    if (0 >= count)
                        return;
                    _ICiImageEvents_SinkHelper aEventSinkHelper;
                    do
                    {
                        aEventSinkHelper = (_ICiImageEvents_SinkHelper)m_aEventSinkHelpers[index];
                        if (aEventSinkHelper.m_LogDelegate == null ||
                            ((aEventSinkHelper.m_LogDelegate.Equals(value) ? 1 : 0) & byte.MaxValue) == 0)
                            ++index;
                        else
                            goto label_8;
                    } while (index < count);

                    return;
                    label_8:
                    m_aEventSinkHelpers.RemoveAt(index);
                    m_ConnectionPoint.Unadvise(aEventSinkHelper.m_dwCookie);
                    if (count > 1)
                        return;
                    Marshal.ReleaseComObject(m_ConnectionPoint);
                    m_ConnectionPoint = null;
                    m_aEventSinkHelpers = null;
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public virtual void Dispose()
        {
            Monitor.Enter(this);
            try
            {
                if (m_ConnectionPoint == null)
                    return;
                var count = m_aEventSinkHelpers.Count;
                var index = 0;
                if (0 < count)
                    do
                    {
                        m_ConnectionPoint.Unadvise(((_ICiImageEvents_SinkHelper)m_aEventSinkHelpers[index]).m_dwCookie);
                        ++index;
                    } while (index < count);

                Marshal.ReleaseComObject(m_ConnectionPoint);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Monitor.Exit(this);
            }

            GC.SuppressFinalize(this);
        }

        private void Init()
        {
            var ppCP = (IConnectionPoint)null;
            var riid = new Guid(new byte[16]
            {
                139,
                241,
                188,
                242,
                39,
                11,
                212,
                17,
                181,
                245,
                156,
                199,
                103,
                0,
                0,
                0
            });
            m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
            m_ConnectionPoint = ppCP;
            m_aEventSinkHelpers = new ArrayList();
        }
    }
}