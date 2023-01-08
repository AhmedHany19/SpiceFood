using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModel;
using Spice.Utility;
using Stripe;
using System.Collections.Generic;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public OrderDetailsCartViewModel OrderDetailsCartVM { get; set; }

        public CartsController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new OrderHeader()
            };
            OrderDetailsCartVM.OrderHeader.OrderTotal= 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var shoppingCart = _db.ShoppingCarts.Where(x => x.ApplicationUserId == claim.Value).ToList();
            if (shoppingCart != null)
            {
                OrderDetailsCartVM.ShoppingCarts=shoppingCart;
            }

            foreach (var item in OrderDetailsCartVM.ShoppingCarts)
            {
                item.MenuItem = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == item.MenuItemId);
                OrderDetailsCartVM.OrderHeader.OrderTotal += item.MenuItem.Price * item.Count;
                item.MenuItem.Description = SD.ConvertToRawHtml(item.MenuItem.Description);

                if (item.MenuItem.Description.Length > 100)
                {
                    item.MenuItem.Description = item.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            OrderDetailsCartVM.OrderHeader.OrderTotalOriginal = OrderDetailsCartVM.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOriginal);
            }

            return View(OrderDetailsCartVM);
        }






        public async Task<IActionResult> Summary()
        {
            OrderDetailsCartVM = new OrderDetailsCartViewModel()
            {
                OrderHeader = new OrderHeader()
            };
            OrderDetailsCartVM.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var appUser = _db.ApplicationUsers.Find(claim.Value);

            OrderDetailsCartVM.OrderHeader.PickUpName = appUser.Name;
            OrderDetailsCartVM.OrderHeader.PhoneNumber = appUser.PhoneNumber;
            OrderDetailsCartVM.OrderHeader.PickUpTime = DateTime.Now;


            var shoppingCart = _db.ShoppingCarts.Where(x => x.ApplicationUserId == claim.Value).ToList();
            if (shoppingCart != null)
            {
                OrderDetailsCartVM.ShoppingCarts = shoppingCart;
            }

            foreach (var item in OrderDetailsCartVM.ShoppingCarts)
            {
                item.MenuItem = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == item.MenuItemId);
                OrderDetailsCartVM.OrderHeader.OrderTotal += item.MenuItem.Price * item.Count;
                
            }

            OrderDetailsCartVM.OrderHeader.OrderTotalOriginal = OrderDetailsCartVM.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOriginal);
            }

            return View(OrderDetailsCartVM);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string? stripeToken)
        {
           
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);




            OrderDetailsCartVM.ShoppingCarts= await _db.ShoppingCarts.Where(x=>x.ApplicationUserId==claim.Value).ToListAsync();
            OrderDetailsCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            OrderDetailsCartVM.OrderHeader.OrderDate = DateTime.Now;
            OrderDetailsCartVM.OrderHeader.UserId = claim.Value;
            OrderDetailsCartVM.OrderHeader.Status = SD.PaymentStatusPending;
            OrderDetailsCartVM.OrderHeader.OrderTotalOriginal =0;

            OrderDetailsCartVM.OrderHeader.PickUpTime = Convert.ToDateTime(
                OrderDetailsCartVM.OrderHeader.PickUpDate.ToShortDateString() 
                + " " +
                OrderDetailsCartVM.OrderHeader.PickUpTime.ToShortTimeString());

            await _db.OrderHeaders.AddAsync(OrderDetailsCartVM.OrderHeader);
            await _db.SaveChangesAsync();






            

            foreach (var item in OrderDetailsCartVM.ShoppingCarts)
            {
                item.MenuItem = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == item.MenuItemId);
                OrderDetail orderDetail = new OrderDetail()
                {
                    MenuItemId= item.MenuItemId,
                    OrderId= OrderDetailsCartVM.OrderHeader.Id,
                    Description=item.MenuItem.Description,
                    Name=item.MenuItem.Name,
                    Price=item.MenuItem.Price,
                    Count=item.Count
                };

                OrderDetailsCartVM.OrderHeader.OrderTotalOriginal+= item.MenuItem.Price * item.Count;
                _db.Add(orderDetail);

            }


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, OrderDetailsCartVM.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                OrderDetailsCartVM.OrderHeader.OrderTotal = Math.Round(OrderDetailsCartVM.OrderHeader.OrderTotalOriginal, 2);
            }


            OrderDetailsCartVM.OrderHeader.CouponCodeDiscount = OrderDetailsCartVM.OrderHeader.OrderTotalOriginal - OrderDetailsCartVM.OrderHeader.OrderTotal;


            _db.ShoppingCarts.RemoveRange(OrderDetailsCartVM.ShoppingCarts);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
           await  _db.SaveChangesAsync();


            // This part About Payment Method Stripe for Testing

            var options = new Stripe.ChargeCreateOptions
            {
                Amount= Convert.ToInt32(OrderDetailsCartVM.OrderHeader.OrderTotal*100),
                Currency= "usd",
                Description= "Order ID : " +OrderDetailsCartVM.OrderHeader.Id,
                Source= stripeToken

            };



            var service = new ChargeService();
            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                OrderDetailsCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                OrderDetailsCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                //await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email, "Spice - Order Created " + OrderDetailsCartVM.OrderHeader.Id.ToString(), "Order has been submitted successfully.");

                OrderDetailsCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                OrderDetailsCartVM.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                OrderDetailsCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Confirm","Orders",new {id =OrderDetailsCartVM.OrderHeader.Id});
        }






        [HttpPost]
        public IActionResult AddCoupon()
        {
            if (OrderDetailsCartVM.OrderHeader.CouponCode == null)
            {
                OrderDetailsCartVM.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, OrderDetailsCartVM.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCarts.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCarts.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCarts.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
