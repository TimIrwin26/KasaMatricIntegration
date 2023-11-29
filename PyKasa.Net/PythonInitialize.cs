using Python.Runtime;

namespace PyKasa.Net
{
    public static class PythonEnvironment
    {
        private static bool _initialized;
        public static void StartUp(string pythonDll)
        {
            if (_initialized) return;

            // should be in config
            Runtime.PythonDLL = pythonDll;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            _initialized = true;
        }

        public static void Shutdown()
        {
/*            if (!_initialized) return;
            using (Py.GIL())
                PythonEngine.Shutdown();

            _initialized = false;*/
        }
    }
}
