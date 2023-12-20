using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KasaMatricIntegration.MatricIntegration.Tests
{
    [TestClass()]
    public class ControlInteractionDataTests
    {
        private const string ControlEventJson =
            @"{
                ""MessageData"":
                {
                    ""ControlId"":""287f2b2b-c3c6-4e5d-a93c-7f63e9ef668b"",
                    ""ControlName"":""Kasa_166"",
                    ""PageId"":""b40c4bed-66ed-4746-96ee-8ccb760488ae"",
                    ""EventName"":""press"",""Data"":null,
                    ""DeckId"":""dcf17922-fef0-4539-8943-17de4e3e36c8""
                },
                ""ClientInfo"":
                {
                    ""clientName"":""Pixel 6"",
                    ""IP"":null,
                    ""clientId"":""7pk4NLSxS/cJ2WbsGFQsxpje9b6bSSMj7OlC6vBoZGU="",
                    ""matricVersion"":2,
                    ""LastContact"":""0001-01-01T00:00:00""
                },
                ""MessageType"":""controlevent""
            }";

        [TestMethod()]
        public void ParseTestControlInteraction()
        {
            ControlInteractionData data = ControlInteractionData.Parse(ControlEventJson);
            Assert.IsNotNull(data);
            Assert.AreEqual(ControlInteractionData.EventMessageType, data.MessageType);
            Assert.IsNotNull(data.MessageData);
            Assert.AreEqual("Kasa_166", data.MessageData.ControlName);
            Assert.AreEqual("Pixel 6", data?.ClientInfo?.Name);
        }
    }
}