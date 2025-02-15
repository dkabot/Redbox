using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua
{
    public class LuaUserData : LuaObject
    {
        private readonly byte[] m_buffer;

        public LuaUserData(LuaVirtualMachine virtualMachine, uint size)
            : base(virtualMachine, new int?())
        {
            m_buffer = new byte[(int)size];
        }

        public LuaUserData(LuaVirtualMachine virtualMachine, int reference)
            : base(virtualMachine, reference)
        {
        }

        public byte this[int offset]
        {
            get
            {
                if (!Reference.HasValue)
                    return m_buffer[offset];
                var userDataPointer = GetUserDataPointer();
                return userDataPointer == IntPtr.Zero ? (byte)0 : Marshal.ReadByte(userDataPointer, offset);
            }
            set
            {
                if (Reference.HasValue)
                {
                    var userDataPointer = GetUserDataPointer();
                    if (userDataPointer == IntPtr.Zero)
                        return;
                    Marshal.WriteByte(userDataPointer, offset, value);
                }
                else
                {
                    m_buffer[offset] = value;
                }
            }
        }

        public override string ToString()
        {
            return "(userdata)";
        }

        public override void Push()
        {
            if (Reference.HasValue)
                base.Push();
            else
                Marshal.Copy(m_buffer, 0, VirtualMachineInstance.Stack.NewUserData(m_buffer.Length), m_buffer.Length);
        }

        public byte[] Read(int offset, int count)
        {
            var numArray = new byte[count];
            if (Reference.HasValue)
            {
                var userDataPointer = GetUserDataPointer();
                if (userDataPointer != IntPtr.Zero)
                    Marshal.Copy(new IntPtr(userDataPointer.ToInt32() + offset), numArray, 0, count);
            }
            else
            {
                Array.Copy(m_buffer, offset, numArray, 0, count);
            }

            return numArray;
        }

        public short ReadInt16(int offset)
        {
            if (!Reference.HasValue)
                return BitConverter.ToInt16(m_buffer, offset);
            var userDataPointer = GetUserDataPointer();
            return !(userDataPointer == IntPtr.Zero)
                ? Marshal.ReadInt16(new IntPtr(userDataPointer.ToInt32() + offset))
                : (short)0;
        }

        public int ReadInt32(int offset)
        {
            if (!Reference.HasValue)
                return BitConverter.ToInt32(m_buffer, offset);
            var userDataPointer = GetUserDataPointer();
            return !(userDataPointer == IntPtr.Zero)
                ? Marshal.ReadInt32(new IntPtr(userDataPointer.ToInt32() + offset))
                : 0;
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            if (Reference.HasValue)
            {
                var userDataPointer = GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.Copy(bytes, 0, new IntPtr(userDataPointer.ToInt32() + offset), count);
            }
            else
            {
                Array.Copy(bytes, 0, m_buffer, offset, count);
            }
        }

        public void WriteInt16(int offset, short value)
        {
            if (Reference.HasValue)
            {
                var userDataPointer = GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.WriteInt16(userDataPointer, offset, value);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes(value), 0, m_buffer, offset, 2);
            }
        }

        public void WriteInt32(int offset, int value)
        {
            if (Reference.HasValue)
            {
                var userDataPointer = GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.WriteInt32(userDataPointer, offset, value);
            }
            else
            {
                Array.Copy(BitConverter.GetBytes(value), 0, m_buffer, offset, 4);
            }
        }

        private IntPtr GetUserDataPointer()
        {
            if (!Reference.HasValue)
                return IntPtr.Zero;
            VirtualMachineInstance.Stack.GetReference(Reference.Value);
            var userData = VirtualMachineInstance.Stack.ToUserData(1);
            VirtualMachineInstance.Stack.Pop();
            return userData;
        }
    }
}