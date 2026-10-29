using System;

namespace MbdPay
{
    /// <summary>
    /// 面包多Pay SDK 异常
    /// </summary>
    public class MbdPayException : Exception
    {
        public MbdPayException(string message) : base(message) { }
        public MbdPayException(string message, Exception innerException) : base(message, innerException) { }
    }
}
