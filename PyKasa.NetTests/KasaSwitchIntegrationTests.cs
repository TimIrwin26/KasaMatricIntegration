using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PyKasa.Net.Tests
{
    [TestClass()]
    public class KasaSwitchIntegrationTests
    {
        private const string PythonDll = "python312.dll";
        private const string TargetIpAddress = "192.168.10.166";

        [TestMethod()]
        public void TurnOnOffTest()
        {
            using (var kasaSwitch = KasaSwitch.Factory(PythonDll, TargetIpAddress))
            {
                kasaSwitch.TurnOn();
                Assert.IsTrue(kasaSwitch.IsOn);
                kasaSwitch.TurnOff();
                Assert.IsFalse(kasaSwitch.IsOn);
            }

            using (var kasaSwitch2 = KasaSwitch.Factory(PythonDll, TargetIpAddress))
            {
                kasaSwitch2.TurnOn();
                Assert.IsTrue(kasaSwitch2.IsOn);
                kasaSwitch2.TurnOff();
                Assert.IsFalse(kasaSwitch2.IsOn);
            }
        }
    }
}