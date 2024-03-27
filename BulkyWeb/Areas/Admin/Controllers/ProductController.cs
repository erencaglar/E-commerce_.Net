using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            List<Product> products = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ProductVM productVM = new ProductVM
            {
                Product = new Product()
                ,
                CategoryList = CategoryList
            };
            if (id == 0 || id == null)
            {
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.ProductRepository
                    .GetFirstOrDefault(p => p.Id == id);
                return View(productVM);
            }

        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? imageUrl)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (imageUrl != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageUrl.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageURL))
                    { //new image uploaded
                        var oldImagepath = Path.Combine(wwwRootPath, obj.Product.ImageURL.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagepath))
                        {
                            System.IO.File.Delete(oldImagepath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        imageUrl.CopyTo(fileStream);
                    }
                    obj.Product.ImageURL = @"\images\product\" + fileName;
                }
                if (obj.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(obj.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = _unitOfWork.CategoryRepository
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });

                return View(obj);
            }
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Product? obj = _unitOfWork.ProductRepository.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(obj.ImageURL))
            { //new image uploaded
                var oldImagepath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageURL.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagepath))
                {
                    System.IO.File.Delete(oldImagepath);
                }
            }
            _unitOfWork.ProductRepository.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfull" });

        }
        #endregion
    }
}
