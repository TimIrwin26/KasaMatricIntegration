
namespace KasaMatricIntegration.Kasa
{
    public interface IKasaDeviceService
    {
        void CheckState(IEnumerable<KasaItem> kasaItems);
        void SwitchDevice(string? id, bool on);
    }
}