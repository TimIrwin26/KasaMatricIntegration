namespace PyKasa.Net
{
    public interface IKasaDeviceFactory : IDisposable
    {
        IKasaDevice CreateDevice(string address = "", string? outlet = null);
    }
}