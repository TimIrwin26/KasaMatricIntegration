using Matric.Integration;

namespace KasaMatricIntegration.Kasa
{
    internal class KasaButton : KasaVariable
    {
        public string? Id { get; set; }

        public SetButtonsVisualStateArgs ToButtonStateArgs()
        {
            return new SetButtonsVisualStateArgs()
            {
                ButtonId = Id,
                ButtonName = Name,
                State = IsOn ? "on" : "off"
            };
        }
    }
}
