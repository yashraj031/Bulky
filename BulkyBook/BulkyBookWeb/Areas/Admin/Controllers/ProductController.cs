using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace BulkyBookWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitofwork _unitofwork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitofwork unitofwork , IWebHostEnvironment hostEnvironment)
        {
            _unitofwork = unitofwork;
            _hostEnvironment = hostEnvironment;

        }
        public IActionResult Index()
        {
           
            return View();
        }
      

        //GET
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _unitofwork.category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.ID.ToString()
                }),

                CoverTypeList = _unitofwork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })

            };


            if (id == null || id == 0)
            {
                //Create Product
                //ViewBag.CoverTypeList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                productVM.product = _unitofwork.Product.GetFirstOrDefault(u=>u.ID == id);
				// Update Product
				return View(productVM);
			}
          
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
           
            if (ModelState.IsValid)
            {
                string wwwRoolPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName= Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRoolPath, @"Images\Products");
                    var extension = Path.GetExtension(file.FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.product.ImageUrl = @"\Images\Products\" + fileName + extension;

                }
                _unitofwork.Product.Add(obj.product);
                _unitofwork.save();
                TempData["success"] = "Product Created  Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

        
            var CoverTypeFromDbFirst = _unitofwork.CoverType.GetFirstOrDefault(u => u.Id == id);


            if (CoverTypeFromDbFirst == null)
            {
                return NotFound();
            }

            return View(CoverTypeFromDbFirst);
        }

        //Post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {

            var obj = _unitofwork.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitofwork.CoverType.Remove(obj);
            _unitofwork.save();
            TempData["success"] = "CoverTypeDeleted Successfully";
            return RedirectToAction("Index");
        }
		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
        {
            var productList = _unitofwork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }

		#endregion

	}

}
