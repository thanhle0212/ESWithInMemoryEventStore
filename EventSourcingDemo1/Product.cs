using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingDemo1
{
    public class Product
    {
        private readonly StreamId _streamId;
        private readonly IStreamStore _streamStore;

        public Product(StreamId streamId, IStreamStore streamStore)
        {
            _streamId = streamId;
            _streamStore = streamStore;
        }

        public async Task<Guid> Receive(int quantity)
        {
            var trx = Guid.NewGuid();
            var receive = new Received(trx, quantity, DateTime.UtcNow);
            await _streamStore.AppendToStream(_streamId, ExpectedVersion.Any, new NewStreamMessage(trx, "ProductReceived", JsonConvert.SerializeObject(receive)));
            return trx;
        }


        public async Task<Guid> Ship(int quantity)
        {
            var trx = Guid.NewGuid();
            var ship = new Shipped(trx, quantity, DateTime.UtcNow);
            await _streamStore.AppendToStream(_streamId, ExpectedVersion.Any, new NewStreamMessage(trx, "ProductShipped", JsonConvert.SerializeObject(ship)));
            return trx;
        }

        public async Task Transactions()
        {
            int quantity = 0;
            var endOfStream = false;
            var startVersion = 0;
            while (endOfStream == false)
            {
                var stream = await _streamStore.ReadStreamForwards(_streamId, startVersion,10);
                endOfStream = stream.IsEnd;
                startVersion = stream.NextStreamVersion;

                foreach (var msg in stream.Messages)
                {
                    switch (msg.Type)
                    {
                        case "ProductReceived":
                            var productReceivedJson = await msg.GetJsonData();
                            var received = JsonConvert.DeserializeObject<Received>(productReceivedJson);
                            Console.WriteLine($"Product Received with Quantity: {received.Quantity} @ {received.DateTime} ({received.TransactionId})");
                            quantity += received.Quantity;
                            break;
                        case "ProductShipped":
                            var productShippedJson = await msg.GetJsonData();
                            var shipped = JsonConvert.DeserializeObject<Shipped>(productShippedJson);
                            Console.WriteLine($"Product is Shipped with Quantity: {shipped.Quantity} @ {shipped.DateTime} ({shipped.TransactionId})");
                            quantity -= shipped.Quantity;
                            break;
                    }
                }
            }

            Console.WriteLine($"Quantity: {quantity}");
        }
    }
}
