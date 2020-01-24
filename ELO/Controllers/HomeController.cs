using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using ELO.Models;

namespace ELO.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Url"] = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return View();
        }
        public IActionResult IndexOld()
        {
            ViewData["Url"] = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            return View();
        }

        public IActionResult Mapa()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Programa ELO.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Contáctenos.";

            return View();
        }

		
		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
		
		[HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
