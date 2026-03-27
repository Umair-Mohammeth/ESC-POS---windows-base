using System;

namespace POSWinForms.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
