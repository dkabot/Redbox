using System;

namespace Redbox.IPC.Framework;

[AttributeUsage(AttributeTargets.Method)]
public class CommandFormAttribute : Attribute
{
    public string Name { get; set; }
}