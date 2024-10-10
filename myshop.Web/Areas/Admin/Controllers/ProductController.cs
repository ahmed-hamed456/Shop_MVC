using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;
using myshop.Entities.ViewModels;

namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
       public IActionResult GetData()
        {
            var products = _unitOfWork.Product.GetAll(includeWord : "Category");
            return Json(new { data = products });
        }


        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategotyList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM productVm,IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(RootPath, @"Images\Products\"); 
                    var ext = Path.GetExtension(file.FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + ext), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVm.Product.Image = @"Images\Products\" + fileName + ext;
                }

                _unitOfWork.Product.Add(productVm.Product);
                _unitOfWork.Complete();
                TempData["Create"] = "Item Has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(productVm.Product);
        }

        [HttpGet]
        public IActionResult Edit(int? productId)
        {
            if (productId == null | productId == 0)
            {
                NotFound();
            }

            ProductVM productVM = new ProductVM()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(x => x.ProductId == productId),
                CategotyList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVm,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(RootPath, @"Images\Products\");
                    var ext = Path.GetExtension(file.FileName);

                    if(productVm.Product.Image != null)
                    {
                        var oldimg = Path.Combine(RootPath, productVm.Product.Image.TrimStart('\\'));
                        if(System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + ext), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVm.Product.Image = @"Images\Products\" + fileName + ext;
                }

                _unitOfWork.Product.Update(productVm.Product);
                _unitOfWork.Complete();
                TempData["Update"] = "Item Has Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(productVm.Product);
        }


        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            var product = _unitOfWork.Product.GetFirstOrDefault(x => x.ProductId == id);

            if (product == null)
            {
               return Json(new { success = false, message = "Error While Deleting" });
            }

            _unitOfWork.Product.Remove(product);

            var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, product.Image.TrimStart('\\'));
            if (System.IO.File.Exists(oldimg))
            {
                System.IO.File.Delete(oldimg);
            }

            _unitOfWork.Complete();
            return Json(new { success = true, message = "File Has been Deleted" });
        }
    }
}
