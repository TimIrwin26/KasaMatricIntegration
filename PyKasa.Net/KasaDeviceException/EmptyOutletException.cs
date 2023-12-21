namespace PyKasa.Net.KasaDeviceException
{
    [Serializable]
    internal class EmptyOutletException : Exception
    {
        public EmptyOutletException()
        {
        }

        public EmptyOutletException(string? message)
        {
        }

        public EmptyOutletException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}