using Microsoft.AspNetCore.Mvc;

namespace GamerLFG.Controllers
{
    public class LobbyController : Controller
    {
        // GET: /Lobby
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Lobby/Create
        public IActionResult Create()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return View();
        }

        // POST: /Lobby/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // TODO: Create lobby in Firebase
            // For now, redirect to lobby list
            TempData["Success"] = "Lobby created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Lobby/Details/{id}
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            
            ViewData["LobbyId"] = id;
            return View();
        }

        // API Endpoints for Ajax

        // GET: /Lobby/GetLobbies
        [HttpGet]
        public IActionResult GetLobbies(string? game = null, string? mood = null, string? status = null, int page = 1)
        {
            // TODO: Fetch from Firebase with filters
            var lobbies = new[]
            {
                new { 
                    LobbyId = "1", 
                    Title = "Boss Hunt - Malenia", 
                    Game = "Elden Ring",
                    Description = "Need a tank and a healer. Mic required.",
                    CurrentPlayers = 2,
                    MaxPlayers = 3,
                    Status = "open"
                },
                new { 
                    LobbyId = "2", 
                    Title = "Ranked Grind to Diamond", 
                    Game = "Valorant",
                    Description = "Must have good comms. No toxicity.",
                    CurrentPlayers = 3,
                    MaxPlayers = 5,
                    Status = "open"
                }
            };

            return Json(new { success = true, data = new { lobbies, pagination = new { currentPage = page, totalPages = 1 } } });
        }

        // POST: /Lobby/Apply/{id}
        [HttpPost]
        public IActionResult Apply(string id, [FromBody] dynamic application)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Json(new { success = false, error = "Please log in first" });
            }

            // TODO: Create application in Firebase
            return Json(new { success = true, message = "Application submitted!" });
        }

        // DELETE: /Lobby/CancelApplication/{id}
        [HttpDelete]
        public IActionResult CancelApplication(string id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Json(new { success = false, error = "Please log in first" });
            }

            // TODO: Delete application from Firebase
            return Json(new { success = true, message = "Application cancelled" });
        }
    }
}
