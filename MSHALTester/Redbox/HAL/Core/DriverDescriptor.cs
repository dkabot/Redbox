using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class DriverDescriptor : IDriverDescriptor
{
    public DriverDescriptor(string version, string provider)
        : this(new Version(version), provider)
    {
    }

    public DriverDescriptor(Version version, string provider)
    {
        DriverVersion = version;
        Provider = provider;
    }

    public Version DriverVersion { get; }

    public string Provider { get; }
}