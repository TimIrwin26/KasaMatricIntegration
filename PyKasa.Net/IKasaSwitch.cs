namespace PyKasa.Net
{
    public interface IKasaSwitch
    {
        string Address { get; set; }
        bool IsOn { get; }
        string? Outlet { get; set; }
        int Timeout { get; set; }

        void Dispose();
        bool SwitchDevice(bool on);
        bool ToggleDevice();
        bool TurnOff();
        bool TurnOn();
    }
}