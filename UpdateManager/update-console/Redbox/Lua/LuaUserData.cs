using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua
{
    internal class LuaUserData : LuaObject
    {
        private readonly byte[] m_buffer;

        public LuaUserData(LuaVirtualMachine virtualMachine, uint size)
          : base(virtualMachine, new int?())
        {
            this.m_buffer = new byte[(int)size];
        }

        public LuaUserData(LuaVirtualMachine virtualMachine, int reference)
          : base(virtualMachine, new int?(reference))
        {
        }

        public byte this[int offset]
        {
            get
            {
                if (!this.Reference.HasValue)
                    return this.m_buffer[offset];
                IntPtr userDataPointer = this.GetUserDataPointer();
                return userDataPointer == IntPtr.Zero ? (byte)0 : Marshal.ReadByte(userDataPointer, offset);
            }
            set
            {
                if (this.Reference.HasValue)
                {
                    IntPtr userDataPointer = this.GetUserDataPointer();
                    if (userDataPointer == IntPtr.Zero)
                        return;
                    Marshal.WriteByte(userDataPointer, offset, value);
                }
                else
                    this.m_buffer[offset] = value;
            }
        }

        public override string ToString() => "(userdata)";

        public override void Push()
        {
            if (this.Reference.HasValue)
                base.Push();
            else
                Marshal.Copy(this.m_buffer, 0, this.VirtualMachineInstance.Stack.NewUserData(this.m_buffer.Length), this.m_buffer.Length);
        }

        public byte[] Read(int offset, int count)
        {
            byte[] numArray = new byte[count];
            if (this.Reference.HasValue)
            {
                IntPtr userDataPointer = this.GetUserDataPointer();
                if (userDataPointer != IntPtr.Zero)
                    Marshal.Copy(new IntPtr(userDataPointer.ToInt32() + offset), numArray, 0, count);
            }
            else
                Array.Copy((Array)this.m_buffer, offset, (Array)numArray, 0, count);
            return numArray;
        }

        public short ReadInt16(int offset)
        {
            if (!this.Reference.HasValue)
                return BitConverter.ToInt16(this.m_buffer, offset);
            IntPtr userDataPointer = this.GetUserDataPointer();
            return !(userDataPointer == IntPtr.Zero) ? Marshal.ReadInt16(new IntPtr(userDataPointer.ToInt32() + offset)) : (short)0;
        }

        public int ReadInt32(int offset)
        {
            if (!this.Reference.HasValue)
                return BitConverter.ToInt32(this.m_buffer, offset);
            IntPtr userDataPointer = this.GetUserDataPointer();
            return !(userDataPointer == IntPtr.Zero) ? Marshal.ReadInt32(new IntPtr(userDataPointer.ToInt32() + offset)) : 0;
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            if (this.Reference.HasValue)
            {
                IntPtr userDataPointer = this.GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.Copy(bytes, 0, new IntPtr(userDataPointer.ToInt32() + offset), count);
            }
            else
                Array.Copy((Array)bytes, 0, (Array)this.m_buffer, offset, count);
        }

        public void WriteInt16(int offset, short value)
        {
            if (this.Reference.HasValue)
            {
                IntPtr userDataPointer = this.GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.WriteInt16(userDataPointer, offset, value);
            }
            else
                Array.Copy((Array)BitConverter.GetBytes(value), 0, (Array)this.m_buffer, offset, 2);
        }

        public void WriteInt32(int offset, int value)
        {
            if (this.Reference.HasValue)
            {
                IntPtr userDataPointer = this.GetUserDataPointer();
                if (userDataPointer == IntPtr.Zero)
                    return;
                Marshal.WriteInt32(userDataPointer, offset, value);
            }
            else
                Array.Copy((Array)BitConverter.GetBytes(value), 0, (Array)this.m_buffer, offset, 4);
        }

        private IntPtr GetUserDataPointer()
        {
            if (!this.Reference.HasValue)
                return IntPtr.Zero;
            this.VirtualMachineInstance.Stack.GetReference(this.Reference.Value);
            IntPtr userData = this.VirtualMachineInstance.Stack.ToUserData(1);
            this.VirtualMachineInstance.Stack.Pop();
            return userData;
        }
    }
}
