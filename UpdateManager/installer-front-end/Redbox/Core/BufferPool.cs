using System.Collections.Generic;

namespace Redbox.Core
{
    internal class BufferPool
    {
        private int m_bufferSize;
        private int m_numberOfBuffers;
        private Queue<byte[]> m_freeBuffers;

        public static BufferPool Instance => Singleton<BufferPool>.Instance;

        public void Initialize(int bufferSize, int numberOfBuffers)
        {
            this.m_bufferSize = bufferSize;
            this.m_numberOfBuffers = numberOfBuffers;
            this.m_freeBuffers = new Queue<byte[]>(this.m_numberOfBuffers);
            for (int index = 0; index < this.m_numberOfBuffers; ++index)
                this.m_freeBuffers.Enqueue(new byte[this.m_bufferSize]);
        }

        public void Shutdown()
        {
            lock (this.m_freeBuffers)
                this.m_freeBuffers.Clear();
        }

        public byte[] Checkout()
        {
            if (this.m_freeBuffers.Count > 0)
            {
                lock (this.m_freeBuffers)
                {
                    if (this.m_freeBuffers.Count > 0)
                        return this.m_freeBuffers.Dequeue();
                }
            }
            return new byte[this.m_bufferSize];
        }

        public void Checkin(byte[] buffer)
        {
            if (buffer == null)
                return;
            lock (this.m_freeBuffers)
            {
                if (this.m_freeBuffers.Contains(buffer))
                    return;
                this.m_freeBuffers.Enqueue(buffer);
            }
        }

        private BufferPool()
        {
        }
    }
}
