using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Raccoons.Networking.Api.WebRequests
{

    public abstract class BaseWebRequestCreator : IWebRequestCreator
    {
        public abstract IWebRequestBuilder Get(string url);

        public abstract IWebRequestBuilder Delete(string url);

        public abstract IWebRequestBuilder Head(string url);

        public abstract IWebRequestBuilder Post(string url);

        public abstract IWebRequestBuilder Put(string url);

    }
}
