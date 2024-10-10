using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess.Data;
using myshop.Entities.Models;
using myshop.Entities.Repositories.Contract;


namespace myshop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Complete();
                TempData["Create"] = "Data Has Created Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }

            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Complete();
                TempData["Update"] = "Data Has Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            return View(category);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {
            var category = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            if (category == null)
            {
                NotFound();
            }

            _unitOfWork.Category.Remove(category);
            _unitOfWork.Complete();
            TempData["Delete"] = "Data Has Deleted Successfully";
            return RedirectToAction("Index");
        }
    }
}
