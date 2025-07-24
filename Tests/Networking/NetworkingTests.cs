using System.Collections;
using System.Collections.Generic;
using Raccoons.Networking.Api.Clients;
using Raccoons.Networking.Api.Configs;
using Raccoons.Networking.Api.WebRequests;
using Raccoons.Networking.Api.WebRequests.UnityWebRequests;
using Raccoons.Serialization;
using Raccoons.Serialization.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raccoons.Networking.Tests
{
    public class NetworkingTests
    {
        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator WebRequestTestsSimplePasses()
        {

            System.Threading.Tasks.Task<IBaseWebResponse> task = new UnityWebRequestBuilder().Get("google.com").Send();
            yield return new WaitUntil(()=>task.IsCompleted);
            var response = task.Result;
            Assert.IsTrue(response.Code == 200);
        }
    }
}