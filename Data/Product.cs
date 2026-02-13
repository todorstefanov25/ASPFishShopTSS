using System.ComponentModel.DataAnnotations;

namespace FishShopASP.Data
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public int CatalogNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Category Categories { get; set; }

        public string Description { get; set; }
        public string imageURL { get; set; }
        [Required]
        public DateTime RegOn { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
         


    }
}
