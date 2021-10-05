using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Threading.Tasks;

namespace EventSourcingDemo1
{
    class Program
    {
        private static InMemoryStreamStore _streamStore;
        private static Product _product;
        private static ProductQuantityProjection _productQuantityProjection;

        static async Task Main()
        {
            var streamId = new StreamId($"Product:{Guid.NewGuid()}");
            _streamStore = new InMemoryStreamStore();
            _product = new Product( streamId, _streamStore);
            _productQuantityProjection = new ProductQuantityProjection(_streamStore, streamId);

            var key = string.Empty;
            while (key != "X")
            {
                Console.WriteLine("I: Import Product");
                Console.WriteLine("O: Order Product");
                Console.WriteLine("Q: Current Quantity of Product");
                Console.WriteLine("T: List all Events");
                Console.WriteLine("X: Exit");
                Console.Write("> ");
                key = Console.ReadLine()?.ToUpperInvariant();
                Console.WriteLine();

                switch (key)
                {
                    case "I":
                        var receivedQuantity = GetQuantity();
                        if (receivedQuantity.IsValid)
                        {
                            var receivedTrx = await _product.Receive(receivedQuantity.Quantity);
                            Console.WriteLine($"Received Quantity: {receivedQuantity.Quantity} ({receivedTrx})");
                        }
                        break;
                    case "O":
                        var shippedQuantity = GetQuantity();
                        if (shippedQuantity.IsValid)
                        {
                            var shippedTrx = await _product.Ship(shippedQuantity.Quantity);
                            Console.WriteLine($"Shipped Quantity: {shippedQuantity.Quantity} ({shippedTrx})");
                        }
                        break;
                    case "Q":
                        ProductQuantity();
                        break;
                    case "T":
                        await _product.Transactions();
                        break;
                }

                Console.WriteLine();
            }
        }

        private static (int Quantity, bool IsValid) GetQuantity()
        {
            Console.Write("Quantity: ");
            if (int.TryParse(Console.ReadLine(), out var quantity))
            {
                return (quantity, true);
            }

            Console.WriteLine("Invalid Quantity.");
            return (0, false);
        }

        private static void ProductQuantity()
        {
            Console.WriteLine($"Product Quantity is: {_productQuantityProjection.ProductQuantity.Quantity} as of {_productQuantityProjection.ProductQuantity.CreatedDate}");
        }
    }
}
