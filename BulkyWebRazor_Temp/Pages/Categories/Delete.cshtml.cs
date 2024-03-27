using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Category category { get; set; }
        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet(int id)
        {
            category = _db.Categories.FirstOrDefault(c => c.Id == id);
        }
        public IActionResult OnPost()
        {
            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "deleted successfully";
            return RedirectToPage("/categories/index");
        }
    }
}
