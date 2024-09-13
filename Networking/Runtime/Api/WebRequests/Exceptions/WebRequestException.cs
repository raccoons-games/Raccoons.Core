using Raccoons.Networking.Api.WebRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Networking.Runtime.Api.WebRequests.Exceptions
{
    public class WebRequestException : Exception
    {
        public IBaseWebResponse WebResponse { get; private set; }

        public WebRequestException(IBaseWebResponse webResponse, string message = "") 
            : base(message)
        {
            WebResponse = webResponse;
            Debug.LogError(Message);
        }
    }
}
