using System;

namespace Redbox.HAL.Core;

public class SingletonException(string message) : ApplicationException(message)
{
}