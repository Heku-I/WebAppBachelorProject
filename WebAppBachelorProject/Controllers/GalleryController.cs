using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAppBachelorProject.Controllers
{
    public class GalleryController : Controller
    {
        // GET: Gallery

        [Authorize]
        public IActionResult Index()
        {
            return View();
            //    try
            //    {
            //        ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            //        //"_context" must be either ImageDb or application???
            //        var gallery = from s in _context.Gallery
            //                      select s;
            //        if (!String.IsNullOrEmpty(searchString))
            //        {
            //            gallery = gallery.Where(s => s.Title.Contains(searchString)
            //                                         || s.Description.Contains(searchString));
            //        }

            //        switch (sortOrder)
            //        {
            //            case "name_desc":
            //                gallery = gallery.OrderByDescending(s => s.Title);
            //                break;
            //            default:
            //                gallery = gallery.OrderBy(s => s.Title);
            //                break;
            //        }

            //        if (gallery == null || !gallery.Any())
            //        {
            //            ViewBag.ErrorMessage = "No images found.";
            //            return View();
            //        }

            //        return View(gallery.ToList() as string);
            //    }
            //    catch (Exception ex)
            //    {
            //        ViewBag.ErrorMessage = "An error has occured while retrieving the gallery";
            //        return View();
            //    }
            //}


        }


        // GET: Gallery/Details/5
        [Authorize]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Gallery/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Gallery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Gallery/Edit/5
        [Authorize]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Gallery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]

        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Gallery/Delete/5
        [Authorize]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Gallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}


