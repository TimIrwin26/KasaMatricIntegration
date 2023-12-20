using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Python.Runtime.Py;

namespace PyKasa.Net
{
    internal class KasaCallEnvironment : IDisposable
    {
        private readonly GILState _gilState;
        public dynamic AsyncIo { get; }
        public dynamic Kasa { get; }
        public dynamic Runner { get; }

        private KasaCallEnvironment(GILState gilState, dynamic asyncio, dynamic kasa, dynamic runner)
        {
            _gilState = gilState;
            AsyncIo = asyncio;
            Runner = runner;
            Kasa = kasa;
        }

        public static KasaCallEnvironment CreateEnvironment()
        {
            var state = GIL();
            dynamic asyncio = Import("asyncio");
            var kasa = Import("kasa");
            asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy());
            var runner = asyncio.Runner();

            return new KasaCallEnvironment(state, asyncio, kasa, runner);
        }

        public void Dispose()
        {
            _gilState.Dispose();
        }
    }
}
