using Python.Runtime;

namespace PyKasa.Net
{
    public static class PythonEnvironment
    {
        private static bool _initialized = false;
        public static void StartUp(string pythonDLL)
        {
            if (_initialized) return;

            // should be in config
            Runtime.PythonDLL = pythonDLL;
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
