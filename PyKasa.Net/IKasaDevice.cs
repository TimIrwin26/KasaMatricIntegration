namespace PyKasa.Net
{
    public interface IKasaDevice
    {
        string Address { get; set; }
        bool IsOn { get; }
        string? Outlet { get; set; }
        int Timeout { get; set; }

        bool SwitchDevice(bool on);
        bool ToggleDevice();
        bool TurnOff();
        bool TurnOn();
    }
}