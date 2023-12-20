using Python.Runtime;

namespace PyKasa.Net
{
    public static class PythonEnvironment
    {
        public static void StartUp(string pythonDll)
        {
            if (PythonEngine.IsInitialized) return;

            // should be in config
            Runtime.PythonDLL = pythonDll;
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        public static void Shutdown()
        {
            PythonEngine.Shutdown();
        }
    }
}
