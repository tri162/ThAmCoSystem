using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThreeAmigosWebApp.Models;
using Microsoft.AspNetCore.Identity;


namespace ThreeAmigosWebApp.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffDashboardController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;

        public StaffDashboardController(UserManager<User> userManager, IOrderService orderService)
        {
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userListWithRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Admin") && !roles.Contains("Staff"))
                {
                    userListWithRoles.Add(new UserWithRolesViewModel
                    {
                        User = user,
                        Roles = roles.ToList()
                    });
                }
            }

            return View(userListWithRoles);
        }

        public async Task<IActionResult> OrdersToDispatch()
        {
            var orders = await _orderService.GetOrdersToDispatchAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> DispatchOrder(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null && !order.IsDispatched)
            {
                order.IsDispatched = true;
                order.DispatchedDate = DateTime.Now;
                await _orderService.UpdateOrderAsync(order);
            }
            return RedirectToAction(nameof(OrdersToDispatch));
        }

        public async Task<IActionResult> ViewCustomerProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            var model = new CustomerProfileViewModel
            {
                User = user,
                Orders = orders
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCustomerProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditCustomerProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCustomerProfile(EditCustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ViewCustomerProfile), new { userId = model.UserId });
            }

            ModelState.AddModelError("", "An error occurred while updating the profile.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomerAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = "DeletedUser_" + user.Id;
            await _userManager.UpdateAsync(user);

            await _orderService.DeleteOrdersByUserIdAsync(userId);

            return RedirectToAction("Index");
        }
    }
}
