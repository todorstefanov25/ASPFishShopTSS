using FishShopASP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FishShopASP.Controllers
{
    [Authorize]
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Client> _userManager;

        private static readonly Dictionary<string, List<OrderItem>> _savedOrders = new();

        public OrderItemsController(ApplicationDbContext context, UserManager<Client> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.OrderItems
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .Where(o => o.ClientId == user.Id)
                .OrderByDescending(o => o.RegOn)
                .ToListAsync();

            return View(cartItems);
        }

        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (_savedOrders.ContainsKey(user.Id))
            {
                var myOrders = _savedOrders[user.Id]
                    .OrderByDescending(o => o.RegOn)
                    .ToList();

                return View(myOrders);
            }

            return View(new List<OrderItem>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            var existingItem = await _context.OrderItems
                .FirstOrDefaultAsync(o => o.ClientId == user.Id && o.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.RegOn = DateTime.Now;
            }
            else
            {
                var orderItem = new OrderItem
                {
                    ClientId = user.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    RegOn = DateTime.Now
                };

                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Продуктът беше добавен в количката.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder()
        {
            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.OrderItems
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .Where(o => o.ClientId == user.Id)
                .ToListAsync();

            if (cartItems.Any())
            {
                if (!_savedOrders.ContainsKey(user.Id))
                {
                    _savedOrders[user.Id] = new List<OrderItem>();
                }

                foreach (var item in cartItems)
                {
                    _savedOrders[user.Id].Add(new OrderItem
                    {
                        Id = item.Id,
                        ClientId = item.ClientId,
                        Clients = item.Clients,
                        ProductId = item.ProductId,
                        Products = item.Products,
                        Quantity = item.Quantity,
                        RegOn = DateTime.Now
                    });
                }

                _context.OrderItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Успешно направихте поръчка.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}