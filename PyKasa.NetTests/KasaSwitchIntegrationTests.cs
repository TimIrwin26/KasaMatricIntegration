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
            using var factory = new KasaDeviceFactory(PythonDll);
            var kasaSwitch = factory.CreateDevice(TargetIpAddress);

            kasaSwitch.TurnOn();
            Assert.IsTrue(kasaSwitch.IsOn);
            kasaSwitch.TurnOff();
            Assert.IsFalse(kasaSwitch.IsOn);

            var kasaSwitch2 = factory.CreateDevice(TargetIpAddress);

            kasaSwitch2.TurnOn();
            Assert.IsTrue(kasaSwitch2.IsOn);
            kasaSwitch2.TurnOff();
            Assert.IsFalse(kasaSwitch2.IsOn);
        }
    }
}