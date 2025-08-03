using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuqiaWaterDistribution.Data;
using SuqiaWaterDistribution.Models;

namespace SuqiaWaterDistribution.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriverController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Region)
                .FirstOrDefaultAsync(d => d.UserId == user!.Id);

            if (driver == null)
                return NotFound();

            var todayOrders = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Tank)
                .Where(o => o.DriverId == driver.Id && o.OrderTime.Date == DateTime.Today)
                .ToListAsync();

            // جلب الطلبات المعلقة في منطقة السائق
            var pendingOrders = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Customer)
                .ThenInclude(c => c.Region)
                .Include(o => o.Tank)
                .Where(o => o.Status == "Pending" && o.Customer.RegionId == driver.RegionId)
                .OrderBy(o => o.OrderTime)
                .ToListAsync();

            ViewBag.TodayOrders = todayOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.CompletedToday = todayOrders.Count(o => o.Status == "Delivered");
            ViewBag.PendingToday = todayOrders.Count(o => o.Status == "InDelivery");

            return View(driver);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == user!.Id);

            if (driver == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.Region)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == "Pending" && o.Customer.RegionId == driver.RegionId);

            if (order == null)
            {
                TempData["Error"] = "لا يمكن العثور على الطلب أو أنه غير متاح للقبول";
                return RedirectToAction("Dashboard");
            }

            // ربط السائق بالطلب وتغيير الحالة
            order.DriverId = driver.Id;
            order.Status = "Accepted";
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم قبول الطلب بنجاح";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> AssignedOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == user!.Id);

            if (driver == null)
                return NotFound();

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Tank)
                .Where(o => o.DriverId == driver.Id)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> StartDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == user!.Id);

            if (driver == null)
                return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DriverId == driver.Id);

            if (order == null)
                return NotFound();

            if (order.Status == "Accepted")
            {
                order.Status = "InDelivery";
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم بدء التوصيل";
            }
            else
            {
                TempData["Error"] = "لا يمكن بدء التوصيل في هذه المرحلة";
            }

            return RedirectToAction("AssignedOrders");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteDelivery(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == user!.Id);

            if (driver == null)
                return NotFound();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.DriverId == driver.Id);

            if (order == null)
                return NotFound();

            if (order.Status == "InDelivery")
            {
                order.Status = "Delivered";
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تسليم الطلب بنجاح";
            }
            else
            {
                TempData["Error"] = "لا يمكن تسليم الطلب في هذه المرحلة";
            }

            return RedirectToAction("AssignedOrders");
        }
    }
}
