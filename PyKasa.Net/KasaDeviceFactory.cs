namespace PyKasa.Net
{
    public class KasaDeviceFactory(string pythonDll, int timeout = 20) : IKasaDeviceFactory
    {
        public int Timeout = timeout;

        public IKasaDevice CreateDevice(string address, string? outlet = null)
        {
            PythonEnvironment.StartUp(pythonDll);
            return new KasaDevice(address, outlet, Timeout);
        }

        public void Dispose()
        {
            PythonEnvironment.Shutdown();
            GC.SuppressFinalize(this);
        }
    }
}
