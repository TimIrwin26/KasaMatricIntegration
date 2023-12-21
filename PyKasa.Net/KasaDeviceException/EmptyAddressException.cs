namespace PyKasa.Net.KasaDeviceException
{
    [Serializable]
    internal class EmptyAddressException : Exception
    {
        public EmptyAddressException()
        {
        }

        public EmptyAddressException(string? message = "No address provided for Kasa device") : base(message)
        {
        }

        public EmptyAddressException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}