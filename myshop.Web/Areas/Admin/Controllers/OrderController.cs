using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using myshop.Entities.ViewModels;
using myshop.Utilities;
using Stripe;
using System.Drawing.Printing;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;

             orderHeaders = _unitOfWork.OrderHeader.GetAll(includeWord: "ApplicationUser");

            return Json(new { data = orderHeaders });
        }

        public IActionResult Details(int orderid)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x=>x.Id == orderid,includeWord:"ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x=>x.OrderHeaderId == orderid,includeWord:"Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            orderfromdb.Name = OrderVM.OrderHeader.Name;
            orderfromdb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderfromdb.Address = OrderVM.OrderHeader.Address;
            orderfromdb.City = OrderVM.OrderHeader.City;

            if(OrderVM.OrderHeader.Carrier != null)
            {
                orderfromdb.Carrier = OrderVM.OrderHeader.Carrier; 
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderfromdb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderfromdb);
            _unitOfWork.Complete();

            TempData["Update"] = "Data Has Updated Successfully";
            return RedirectToAction("Details", "Order", new {orderid = orderfromdb.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProccess()
        {
            _unitOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Proccessing, null);
            _unitOfWork.Complete();

            TempData["Update"] = "Order Status Has Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShip()
        {
            var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);
            orderfromdb.Carrier = OrderVM.OrderHeader.Carrier;
            orderfromdb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderfromdb.OrderStatus = SD.Shipped;
            orderfromdb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderfromdb);
            _unitOfWork.Complete();

            TempData["Update"] = "Order Has Shipped Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderfromdb = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);

            if(orderfromdb.PaymentStatus == SD.Approve)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderfromdb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateOrderStatus(orderfromdb.Id, SD.Cancelled, SD.Refund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(orderfromdb.Id, SD.Cancelled, SD.Cancelled);
            }

            _unitOfWork.Complete();

            TempData["Update"] = "Order Has Cancelled Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
    }
}
