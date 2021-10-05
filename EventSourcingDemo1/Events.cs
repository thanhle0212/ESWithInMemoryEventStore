using System;

namespace EventSourcingDemo1
{
    public abstract class ProductEvent
    {
        public Guid TransactionId { get; }
        public int Quantity { get; }
        public DateTime DateTime { get; }

        public ProductEvent(Guid transactionId, int quantity, DateTime dateTime)
        {
            TransactionId = transactionId;
            Quantity = quantity;
            DateTime = dateTime;
        }
    }

    public class Received : ProductEvent
    {
        public Received(Guid transactionId, int quantity, DateTime dateTime) : base(transactionId, quantity, dateTime)
        {

        }
    }

    public class Shipped : ProductEvent
    {
        public Shipped(Guid transactionId, int quantity, DateTime dateTime) : base(transactionId, quantity, dateTime)
        {

        }
    }
}
