using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using myshop.Entities.ViewModels;
using myshop.Utilities;
using Stripe.Checkout;
using System.Security.Claims;

namespace myshop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == userId,includeWord:"Product")
            };

            foreach(var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }


        [HttpGet]
		public IActionResult Summary()
		{
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userId = claim.Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeWord: "Product"),
                OrderHeader =new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;



			foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);

        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult PostSummary(ShoppingCartVM shoppingCartVM)
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			string userId = claim.Value;

            shoppingCartVM.CartsList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeWord: "Product");

            shoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartVM.OrderHeader.PaymentIntentId = SD.Pending;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

			foreach (var item in shoppingCartVM.CartsList)
			{
				shoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
			}

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Complete();


            foreach (var item in shoppingCartVM.CartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Complete();
            }

            var domain = "https://localhost:7017/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
        
                Mode = "payment",
                SuccessUrl = domain+$"customer/cart/orderconfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };

            foreach (var item in shoppingCartVM.CartsList)
            {
               var sessionLineOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineOptions);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartVM.OrderHeader.SessionId = session.Id;

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);

		}

		public IActionResult orderconfirmation(int id)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);

			var service = new SessionService();
			Session session = service.Get(orderHeader.SessionId);

			if (session.PaymentStatus.ToLower() == "paid")
			{
				_unitOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve,SD.Approve);
                orderHeader.PaymentIntentId = session.PaymentIntentId;
				_unitOfWork.Complete();
			}
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Complete();
			return View(id);
		}

		public ActionResult Plus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.ShoppingCartId == cartId);

            _unitOfWork.ShoppingCart.IncreaseCart(shoppingCart, 1);

            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

		public ActionResult Minus(int cartId)
		{
			var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.ShoppingCartId == cartId);

            if(shoppingCart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
				_unitOfWork.ShoppingCart.DecreaseCart(shoppingCart, 1);
			}

			_unitOfWork.Complete();

			return RedirectToAction("Index");
		}

		public ActionResult Remove(int cartId)
		{
			var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.ShoppingCartId == cartId);

			_unitOfWork.ShoppingCart.Remove(shoppingCart);

			_unitOfWork.Complete();

            var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return RedirectToAction("Index");
		}
	}
}
