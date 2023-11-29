using Matric.Integration;

namespace KasaMatricIntegration.Kasa
{
    internal class KasaVariable : KasaItem
    {
        public ServerVariable ToServerVariable()
        {
            return new ServerVariable()
            {
                Name = Name,
                Value = IsOn,
                VariableType = ServerVariable.ServerVariableType.BOOL
            };
        }
    }
}
