using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public class ListServersResponse
    {
        public IList<Server> Servers { get; set; }

        string ErrorMessage { get; set; }

        public ListServersResponse()
        {
        }

        public ListServersResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
