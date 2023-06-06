using Crosscutting;
using System.Web.Mvc;

namespace Study.PostSharp.Controllers
{
    [LogAspect]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
