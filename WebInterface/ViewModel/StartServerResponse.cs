using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public class StartServerResponse
    {
        public bool IsSuccess { get; set; }
        public int Port { get; set; }
        public string CommandLine { get; set; }
        public string ErrorMessage { get; set; }

        public StartServerResponse(string errorMessage = "")
        {
            IsSuccess = false;
            ErrorMessage = errorMessage ?? "";
        }

        public StartServerResponse(int port)
        {
            IsSuccess = true;
            Port = port;
        }
    }
}
