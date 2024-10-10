using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using myshop.Utilities;
using System.Security.Claims;
using X.PagedList;

namespace myshop.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int? page)
        {
            var PageNumber = page ?? 1;
            int PageSize = 3;

            var products = _unitOfWork.Product.GetAll().ToPagedList(PageNumber,PageSize);
            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = id,
                Product = _unitOfWork.Product.GetFirstOrDefault(p => p.ProductId == id, includeWord: "Category"),
                Count = 1
            };

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartObj = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId
                );

            if (cartObj == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Complete();
                HttpContext.Session.SetInt32(SD.SessionKey,
                    _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count()
                    );
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCart(cartObj, shoppingCart.Count);
                _unitOfWork.Complete();

            }


            return RedirectToAction("Index");
        }
    }
}
