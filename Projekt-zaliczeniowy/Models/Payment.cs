namespace Projekt_zaliczeniowy.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public Lesson Lesson { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
    }
}
