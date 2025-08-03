using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuqiaWaterDistribution.Data;
using SuqiaWaterDistribution.Models;

namespace SuqiaWaterDistribution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            var completedOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");
            var totalCustomers = await _context.Customers.CountAsync();
            var totalDrivers = await _context.Drivers.CountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalDrivers = totalDrivers;

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Tank)
                .OrderByDescending(o => o.OrderTime)
                .Take(10)
                .ToListAsync();

            return View(recentOrders);
        }

        // Manage Orders
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.User)
                .Include(o => o.Tank)
                .Include(o => o.Driver)
                .ThenInclude(d => d!.User)
                .OrderByDescending(o => o.OrderTime)
                .ToListAsync();

            return View(orders);
        }

        // In AdminController.cs
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Tank)
                .Include(o => o.Customer).ThenInclude(c => c.User)
                .Include(o => o.Customer).ThenInclude(c => c.Region)
                .Include(o => o.Driver).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Accepted";
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم قبول الطلب";
            }
            return RedirectToAction("ManageOrders");
        }

        [HttpPost]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && (order.Status == "Pending" || order.Status == "Accepted"))
            {
                order.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم رفض الطلب";
            }
            return RedirectToAction("ManageOrders");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الطلب بنجاح";
            return RedirectToAction("ManageOrders");
        }

        // Manage Customers
        public async Task<IActionResult> ManageCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Region)
                .ToListAsync();

            return View(customers);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Region)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.Tank)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomer(Customer customer, bool IsLocked)
        {
            var existingCustomer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingCustomer.User.FullName = customer.User.FullName;
                existingCustomer.User.Email = customer.User.Email;
                existingCustomer.User.PhoneNumber = customer.User.PhoneNumber;
                existingCustomer.User.Address = customer.User.Address;
                existingCustomer.RegionId = customer.RegionId;
                existingCustomer.User.EmailConfirmed = customer.User.EmailConfirmed;

                if (IsLocked)
                {
                    existingCustomer.User.LockoutEnd = DateTimeOffset.MaxValue;
                }
                else
                {
                    existingCustomer.User.LockoutEnd = null;
                }

                _context.Customers.Update(existingCustomer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات العميل بنجاح";
                return RedirectToAction("ManageCustomers");
            }
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            // حذف الطلبات المرتبطة أولاً
            _context.Orders.RemoveRange(customer.Orders);

            // حذف العميل
            _context.Customers.Remove(customer);

            // حذف المستخدم
            _context.Users.Remove(customer.User);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف العميل بنجاح";
            return RedirectToAction("ManageCustomers");
        }


        // Manage Drivers
        public async Task<IActionResult> ManageDrivers()
        {
            var drivers = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Region)
                .ToListAsync();

            return View(drivers);
        }

        [HttpGet]
        public async Task<IActionResult> DriverDetails(int id)
        {
            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Region)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.Customer)
                        .ThenInclude(c => c.User)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.Tank)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);
        }

        [HttpGet]
        public async Task<IActionResult> EditDriver(int id)
        {
            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound();
            }
            ViewBag.Regions = new SelectList(await _context.Regions.ToListAsync(), "Id", "Name", driver.RegionId);
            return View(driver);
        }

        [HttpPost]
        public async Task<IActionResult> EditDriver(Driver driver)
        {
            var existingDriver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == driver.Id);

            if (existingDriver == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingDriver.User.FullName = driver.User.FullName;
                existingDriver.User.Email = driver.User.Email;
                existingDriver.User.PhoneNumber = driver.User.PhoneNumber;
                existingDriver.User.Address = driver.User.Address;
                existingDriver.RegionId = driver.RegionId;

                _context.Drivers.Update(existingDriver);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات السائق بنجاح";
                return RedirectToAction("ManageDrivers");
            }
            ViewBag.Regions = new SelectList(await _context.Regions.ToListAsync(), "Id", "Name", driver.RegionId);
            return View(driver);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var driver = await _context.Drivers
                .Include(d => d.User)
                .Include(d => d.Orders)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound();
            }

            // تحديث الطلبات المرتبطة لإزالة السائق
            foreach (var order in driver.Orders)
            {
                order.DriverId = null;
                order.Status = "Accepted"; // إعادة الطلب لحالة مقبول
            }

            // حذف السائق
            _context.Drivers.Remove(driver);

            // حذف المستخدم
            _context.Users.Remove(driver.User);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف السائق بنجاح";
            return RedirectToAction("ManageDrivers");
        }

        // Manage Regions
        public async Task<IActionResult> ManageRegions()
        {
            var regions = await _context.Regions.ToListAsync();
            return View(regions);
        }

        [HttpGet]
        public async Task<IActionResult> RegionDetails(int id)
        {
            var region = await _context.Regions
                .Include(r => r.Customers)
                    .ThenInclude(c => c.User)
                .Include(r => r.Customers)
                    .ThenInclude(c => c.Orders)
                .Include(r => r.Drivers)
                    .ThenInclude(d => d.User)
                .Include(r => r.Drivers)
                    .ThenInclude(d => d.Orders)
                .Include(r => r.TankRegions)
                    .ThenInclude(tr => tr.Tank)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        [HttpGet]
        public IActionResult CreateRegion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegion(Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Regions.Add(region);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم إضافة المنطقة بنجاح";
                return RedirectToAction("ManageRegions");
            }
            return View(region);
        }

        [HttpGet]
        public async Task<IActionResult> EditRegion(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            return View(region);
        }

        [HttpPost]
        public async Task<IActionResult> EditRegion(Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Regions.Update(region);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث المنطقة بنجاح";
                return RedirectToAction("ManageRegions");
            }
            return View(region);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            var region = await _context.Regions
                .Include(r => r.Customers)
                .Include(r => r.Drivers)
                .Include(r => r.TankRegions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (region == null)
            {
                return NotFound();
            }

            // التحقق من وجود عملاء أو سائقين مرتبطين
            if (region.Customers.Any() || region.Drivers.Any())
            {
                TempData["Error"] = "لا يمكن حذف المنطقة لوجود عملاء أو سائقين مرتبطين بها";
                return RedirectToAction("ManageRegions");
            }

            // حذف العلاقات مع الخزانات
            _context.TankRegions.RemoveRange(region.TankRegions);

            // حذف المنطقة
            _context.Regions.Remove(region);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف المنطقة بنجاح";
            return RedirectToAction("ManageRegions");
        }


        // Manage Tanks
        public async Task<IActionResult> ManageTanks()
        {
            var tanks = await _context.Tanks
                .Include(t => t.TankRegions)
                .ThenInclude(tr => tr.Region)
                .ToListAsync();

            return View(tanks);
        }

        [HttpGet]
        public async Task<IActionResult> TankDetails(int id)
        {
            var tank = await _context.Tanks
                .Include(t => t.TankRegions)
                    .ThenInclude(tr => tr.Region)
                        .ThenInclude(r => r.Customers)
                .Include(t => t.TankRegions)
                    .ThenInclude(tr => tr.Region)
                        .ThenInclude(r => r.Drivers)
                .Include(t => t.Orders)
                    .ThenInclude(o => o.Customer)
                        .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tank == null)
            {
                return NotFound();
            }
            return View(tank);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTank()
        {
            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTank(Tank tank, int[] selectedRegions)
        {
            if (ModelState.IsValid)
            {
                _context.Tanks.Add(tank);
                await _context.SaveChangesAsync();

                // Add tank-region relationships
                if (selectedRegions != null)
                {
                    foreach (var regionId in selectedRegions)
                    {
                        _context.TankRegions.Add(new TankRegion
                        {
                            TankId = tank.Id,
                            RegionId = regionId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "تم إضافة الخزان بنجاح";
                return RedirectToAction("ManageTanks");
            }

            ViewBag.Regions = await _context.Regions.ToListAsync();
            return View(tank);
        }

        [HttpGet]
        public async Task<IActionResult> EditTank(int id)
        {
            var tank = await _context.Tanks.FindAsync(id);
            if (tank == null)
            {
                return NotFound();
            }
            return View(tank);
        }

        [HttpPost]
        public async Task<IActionResult> EditTank(Tank tank)
        {
            if (ModelState.IsValid)
            {
                _context.Tanks.Update(tank);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث الخزان بنجاح";
                return RedirectToAction("ManageTanks");
            }
            return View(tank);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTank(int id)
        {
            var tank = await _context.Tanks
                .Include(t => t.Orders)
                .Include(t => t.TankRegions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tank == null)
            {
                return NotFound();
            }

            // التحقق من وجود طلبات مرتبطة
            if (tank.Orders.Any())
            {
                TempData["Error"] = "لا يمكن حذف الخزان لوجود طلبات مرتبطة به";
                return RedirectToAction("ManageTanks");
            }

            // حذف العلاقات مع المناطق
            _context.TankRegions.RemoveRange(tank.TankRegions);

            // حذف الخزان
            _context.Tanks.Remove(tank);

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حذف الخزان بنجاح";
            return RedirectToAction("ManageTanks");
        }

        // Statistics
        public async Task<IActionResult> Statistics()
        {
            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var ordersByRegion = await _context.Orders
                .Include(o => o.Customer)
                .ThenInclude(c => c.Region)
                .GroupBy(o => o.Customer.Region.Name)
                .Select(g => new { Region = g.Key, Count = g.Count() })
                .ToListAsync();

            var monthlyRaw = await _context.Orders
                .Where(o => o.OrderTime >= DateTime.Now.AddMonths(-6))
                .GroupBy(o => new { o.OrderTime.Year, o.OrderTime.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.Price)
                })
                .ToListAsync();

            var monthlyOrders = monthlyRaw
                .Select(x => new {
                    Month = $"{x.Year}-{x.Month:00}",
                    x.Count,
                    x.Revenue
                })
                .OrderBy(x => x.Month)
                .ToList();

            ViewBag.OrdersByStatus = ordersByStatus;
            ViewBag.OrdersByRegion = ordersByRegion;
            ViewBag.MonthlyOrders = monthlyOrders;

            return View();
        }
    }
}