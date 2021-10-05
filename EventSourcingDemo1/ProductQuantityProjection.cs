using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcingDemo1
{
    public class ProductQuantityProjection
    {
        public ProductQuantity ProductQuantity { get; private set; } = new ProductQuantity(0, DateTime.UtcNow);

        public ProductQuantityProjection(IStreamStore streamStore, StreamId streamId)
        {
            streamStore.SubscribeToStream(streamId, null, StreamMessageReceived);
        }

        private async Task StreamMessageReceived(IStreamSubscription subscription, StreamMessage streamMessage, CancellationToken cancellationToken)
        {
            switch (streamMessage.Type)
            {
                case "ProductReceived":
                    var productReceivedJson = await streamMessage.GetJsonData(cancellationToken);
                    var received = JsonConvert.DeserializeObject<Received>(productReceivedJson);
                    ProductQuantity = ProductQuantity.Add(received.Quantity);
                    break;
                case "ProductShipped":
                    var productShippedJson = await streamMessage.GetJsonData(cancellationToken);
                    var productShipped = JsonConvert.DeserializeObject<Shipped>(productShippedJson);
                    ProductQuantity = ProductQuantity.Subtract(productShipped.Quantity);
                    break;
            }
        }
    }

    public class ProductQuantity
    {
        public int Quantity { get; }
        public DateTime CreatedDate { get; }

        public ProductQuantity(int quantity, DateTime createdDate)
        {
            Quantity = quantity;
            CreatedDate = createdDate;
        }
      
        public ProductQuantity Add(int value)
        {
            return new ProductQuantity(Quantity + value, DateTime.UtcNow);
        }

        public ProductQuantity Subtract(int value)
        {
            return new ProductQuantity(Quantity - value, DateTime.UtcNow);
        }
    }
}
