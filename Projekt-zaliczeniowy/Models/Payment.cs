namespace Projekt_zaliczeniowy.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string? TransactionId { get; set; }

        public virtual Lesson Lesson { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        Cash
    }
}
