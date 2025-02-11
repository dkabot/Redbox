using System;
using System.Security.Cryptography;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class IdHelper
    {
        private readonly RNGCryptoServiceProvider m_provider = new RNGCryptoServiceProvider();

        internal string GeneratePseudoUniqueId()
        {
            return GeneratePseudoUniqueId(8);
        }

        internal string GeneratePseudoUniqueId(int numberOfBytes)
        {
            var data = new byte[numberOfBytes];
            m_provider.GetBytes(data);
            return BaseConverter.Convert(10, 36, Math.Abs(BitConverter.ToInt32(data, 0)).ToString());
        }
    }
}