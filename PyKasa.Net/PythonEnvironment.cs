using Python.Runtime;

namespace PyKasa.Net
{
    public static class PythonEnvironment
    {
        const string PythonPathEnvironment = "PYTHONPATH";
        public static void StartUp(string pythonDll)
        {
            if (PythonEngine.IsInitialized) return;

            var pythonPath = Environment.GetEnvironmentVariable(PythonPathEnvironment);
            pythonPath += $"{Environment.CurrentDirectory}\\Lib\\site-packages";
            Environment.SetEnvironmentVariable(PythonPathEnvironment, pythonPath);

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
