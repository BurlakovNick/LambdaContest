using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeController: Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Viewer()
        {
            return View();
        }
    }
}