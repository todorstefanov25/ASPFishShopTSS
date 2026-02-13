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
        public string ProductId { get; set; }
        public Product Products { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        DateTime RegOn { get; set; }
        
        
        
    }
}
