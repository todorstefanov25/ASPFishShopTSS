using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FishShopASP.Data
{
    public class Client:IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
