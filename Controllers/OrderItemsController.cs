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
                .Where(o => o.ClientId == user.Id && !o.IsCompleted)
                .OrderByDescending(o => o.RegOn)
                .ToListAsync();

            return View(cartItems);
        }

        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            var myOrders = await _context.OrderItems
                .Include(o => o.Clients)
                .Include(o => o.Products)
                .Where(o => o.ClientId == user.Id && o.IsCompleted)
                .OrderByDescending(o => o.RegOn)
                .ToListAsync();

            return View(myOrders);
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
                .FirstOrDefaultAsync(o => o.ClientId == user.Id && o.ProductId == productId && !o.IsCompleted);

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
                    RegOn = DateTime.Now,
                    IsCompleted = false
                };

                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Продуктът беше добавен в количката.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == user.Id && !o.IsCompleted);

            if (orderItem == null)
            {
                return NotFound();
            }

            if (quantity < 1)
            {
                _context.OrderItems.Remove(orderItem);
                TempData["SuccessMessage"] = "Артикулът беше премахнат от количката.";
            }
            else
            {
                orderItem.Quantity = quantity;
                orderItem.RegOn = DateTime.Now;
                TempData["SuccessMessage"] = "Количеството беше обновено.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(o => o.Id == id && o.ClientId == user.Id && !o.IsCompleted);

            if (orderItem == null)
            {
                return NotFound();
            }

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Артикулът беше премахнат от количката.";

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
                .Where(o => o.ClientId == user.Id && !o.IsCompleted)
                .ToListAsync();

            if (cartItems.Any())
            {
                var orderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
                var completedOn = DateTime.Now;

                foreach (var item in cartItems)
                {
                    item.IsCompleted = true;
                    item.OrderNumber = orderNumber;
                    item.RegOn = completedOn;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Успешно направихте поръчка.";
            }

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
