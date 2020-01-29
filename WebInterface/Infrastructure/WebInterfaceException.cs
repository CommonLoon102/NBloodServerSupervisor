using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Infrastructure
{

    [Serializable]
    public class WebInterfaceException : Exception
    {
        public WebInterfaceException() { }
        public WebInterfaceException(string message) : base(message) { }
        public WebInterfaceException(string message, Exception inner) : base(message, inner) { }
        protected WebInterfaceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
