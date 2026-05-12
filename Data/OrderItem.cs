using System.ComponentModel.DataAnnotations;

namespace FishShopASP.Data
{
    public class OrderItem
    {
        public int Id  { get; set; }
        [Required]
        public string ClientId { get; set; }
        public Client Clients { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Products { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public DateTime RegOn { get; set; }
        public bool IsCompleted { get; set; }
        public string? OrderNumber { get; set; }



    }
}
