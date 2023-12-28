
namespace KasaMatricIntegration.Kasa
{
    public interface IKasaDeviceService
    {
        void CheckKasaState(IEnumerable<KasaItem> kasaItems);
        void SwitchDevice(string? id, bool on);
    }
}