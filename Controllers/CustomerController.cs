using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuqiaWaterDistribution.Data;
using SuqiaWaterDistribution.Models;
using SuqiaWaterDistribution.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SuqiaWaterDistribution.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- دالة مساعدة لتحسين الكفاءة ومنع تكرار الكود ---
        private Task<Customer> GetCurrentCustomerAsync()
        {
            var userId = _userManager.GetUserId(User);
            // جلب العميل مع بيانات المستخدم والمنطقة مرة واحدة
            return _context.Customers
                .Include(c => c.User)
                .Include(c => c.Region)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<IActionResult> Dashboard()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound("Customer not found.");

            // جلب آخر 5 طلبات للعميل
            customer.Orders = await _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.OrderTime)
                .Include(o => o.Tank) // تحميل الخزان لعرض اسمه
                .Take(5)
                .ToListAsync();

            return View(customer);
        }

        public async Task<IActionResult> Tanks()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound("Customer not found.");

            var tanks = await _context.Tanks
                .Where(t => t.TankRegions.Any(tr => tr.RegionId == customer.RegionId))
                .ToListAsync();

            return View(tanks);
        }

        // --- قسم إدارة الطلبات ---

        [HttpGet]
        public async Task<IActionResult> CreateOrder(int tankId)
        {
            var tank = await _context.Tanks.FindAsync(tankId);
            if (tank == null) return NotFound();

            var model = new CreateOrderViewModel
            {
                TankId = tankId,
                TankName = tank.Name,
                PricePerBarrel = tank.PricePerBarrel
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = await GetCurrentCustomerAsync();
                if (customer == null) return NotFound();

                var tank = await _context.Tanks.FindAsync(model.TankId);
                if (tank == null) return NotFound();

                var order = new Order
                {
                    CustomerId = customer.Id,
                    TankId = model.TankId,
                    Quantity = model.Quantity,
                    Price = model.Quantity * tank.PricePerBarrel,
                    OrderTime = DateTime.Now,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إرسال الطلب بنجاح";
                return RedirectToAction(nameof(TrackOrders));
            }
            // In case of error, repopulate the view model
            var tankData = await _context.Tanks.FindAsync(model.TankId);
            if (tankData != null)
            {
                model.TankName = tankData.Name;
                model.PricePerBarrel = tankData.PricePerBarrel;
            }
            return View(model);
        }

        public async Task<IActionResult> TrackOrders()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound();

            var orders = await _context.Orders
                .Where(o => o.CustomerId == customer.Id)
                .Include(o => o.Tank)
                .Include(o => o.Driver).ThenInclude(d => d.User)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Tank)
                .Include(o => o.Driver).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

            if (order == null) return NotFound("Order not found or you don't have permission.");

            // بما أننا قمنا بتحميل العميل بالفعل، يمكننا إرفاقه مباشرة
            order.Customer = customer;

            return View(order);
        }
        // ===============================================================

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

            if (order == null) return NotFound();

            if (order.Status == "Pending")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إلغاء الطلب بنجاح";
            }
            else
            {
                TempData["Error"] = "لا يمكن إلغاء الطلب في هذه المرحلة";
            }
            return RedirectToAction(nameof(TrackOrders));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

            if (order == null) return NotFound();

            if (new[] { "InDelivery", "Delivered" }.Contains(order.Status))
            {
                TempData["Error"] = "لا يمكن حذف طلب قيد التنفيذ أو مكتمل.";
                return RedirectToAction(nameof(TrackOrders));
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الطلب بنجاح.";
            return RedirectToAction(nameof(TrackOrders));
        }

        [HttpPost]
        public async Task<IActionResult> RateOrder(int orderId, int rating, string? comment)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null) return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customer.Id);

            if (order == null) return NotFound();

            if (order.Status == "Delivered")
            {
                order.Rating = rating;
                order.Comment = comment;
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تقييم الخدمة بنجاح";
            }
            return RedirectToAction(nameof(TrackOrders));
        }
    }
}