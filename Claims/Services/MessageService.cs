using Claims.Services.Interfaces;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace Claims.Services
{
    public class MessageService : IMessageService
    {
        private const string ServiceBusConnectionString = "Endpoint=sb://claimsaudit.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=MwP1r85x6R16QX+BmiEG4Kdsu1tL3L/2v+ASbKsq54Q=";

        public async Task SendMessage(dynamic obj)
        {
            var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));

            var topicClient = new QueueClient(ServiceBusConnectionString, "audit-claim");

            await topicClient.SendAsync(message);

        }
    }
}
