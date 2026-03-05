using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Models.TestData;
using GamerLFG.Services;
using GamerLFG.Services.Interface.DTOs;
using System.Text.Json;
using GamerLFG.Services.Interface; // สำหรับ ASP.NET Core
using System.Security.Claims;
using GamerLFG.service;
using MongoDB.Driver;
namespace GamerLFG.Controllers
{   
    

    public class LobbyController : Controller
    {
        private readonly ILobbyService _lobbyService;
        public LobbyController(ILobbyService lobbyService,MongoDBservice mongoDBservice)
        {
            _lobbyService = lobbyService;
            _mongoDBservice = mongoDBservice;

        }
        public async Task<IActionResult> Details(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            };
            var viewModel = await _lobbyService.GetLobbyDetailsAsync(id, currentUserId);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }
// ยังไม่ชัวร์
        private readonly MongoDBservice _mongoDBservice;
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var lobbyData = await _lobbyService.GetAllLobbyAsync(userId);
            return View(lobbyData);
        }

        // ── DEV ONLY: จำลอง login เป็น user ที่ต้องการ ─────────────────────────
        // Usage: /Lobby/SwitchUser?userId=000000000000000000000001
        //        /Lobby/SwitchUser          ← logout (clear session)
        [HttpGet]
        public IActionResult SwitchUser(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                HttpContext.Session.Remove("UserId");
            else
                HttpContext.Session.SetString("UserId", userId);

            var lobbyId = LobbySeeder.IdLobby;
            return Redirect($"/Lobby/Details/{lobbyId}");
        }

        // ── DEV ONLY: แสดงหน้า test panel ────────────────────────────────────
        // Usage: /Lobby/TestPanel
        [HttpGet]
        public IActionResult TestPanel()
        {
            var lobbyId = LobbySeeder.IdLobby;
            var users = new[]
            {
                new { Label = "SC1 — HOST", UserId = LobbySeeder.IdHost    },
                new { Label = "SC2 — NOT LOGGED IN", UserId = (string?)null          },
                new { Label = "SC3/SC4 — VISITOR", UserId = LobbySeeder.IdVisitor },
                new { Label = "SC5 — PENDING (Visitor)", UserId = LobbySeeder.IdVisitor },
                new { Label = "SC6/SC7 — MEMBER", UserId = LobbySeeder.IdMember1 },
            };

            var html = $@"
                            <!DOCTYPE html><html>
                            <head><meta charset=""utf-8""><title>Lobby Test Panel</title>
                            <style>body{{background:#111;color:#ddd;font-family:monospace;padding:40px}}
                            a{{display:block;margin:8px 0;padding:12px 20px;background:#1a1a1a;border:1px solid #333;
                            border-radius:8px;color:#f2960d;text-decoration:none;font-size:14px}}
                            a:hover{{background:#222}} h2{{color:#fff}} code{{color:#2ecc71}}</style>
                            </head><body>
                            <h2>🎮 Lobby Test Panel</h2>
                            <p style='color:#888'>Lobby ID: <code>{lobbyId}</code></p>
                            <a href='/Lobby/SwitchUser?userId={LobbySeeder.IdHost}'>SC1 — HOST (active, recruiting open + pending request)</a>
                            <a href='/Lobby/SwitchUser'>SC2 — NOT LOGGED IN</a>
                            <a href='/Lobby/SwitchUser?userId={LobbySeeder.IdVisitor}'>SC3/SC4 — VISITOR (login แล้ว ยังไม่ได้ขอเข้า)</a>
                            <a href='/Lobby/SwitchUser?userId={LobbySeeder.IdPending}'>SC5 — PENDING (ส่ง request ไปแล้ว รอ)</a>
                            <a href='/Lobby/SwitchUser?userId={LobbySeeder.IdMember1}'>SC6 — MEMBER (ถูกรับเข้าแล้ว)</a>
                            <hr style='border-color:#333;margin:20px 0'>
                            <p style='color:#555;font-size:12px'>
                            หมายเหตุ: SC7/SC8 (Completed) ต้องเปลี่ยน IsComplete=true ใน DB ก่อน<br>
                            หรือใช้ MongoDB Compass: set <code>IsComplete: true, IsRecruiting: false</code>
                            </p>
                            </body></html>";
                    return Content(html, "text/html; charset=utf-8");
                            }   

        // --- ส่วนของ API ที่เรียกใช้จาก JavaScript (fetch) ในหน้า View ---


        //ยังไม่ชัวร์ ^^&

        [HttpPost]
        public async Task<IActionResult> Apply(string id, string role)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.ApplyToLobbyAsync(id, currentUserId, role);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(string id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.CancelApplicationAsync(id, currentUserId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Recruit(string id, string userId)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.RecruitMemberAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id, string userId)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.RejectApplicantAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Kick(string id, string userId)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.KickMemberAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> AbandonMission(string id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.KickMemberAsync(id, currentUserId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMission(string id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.CompleteLobbyAsync(id);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> TerminateLobby(string id)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.DeleteLobbyAsync(id);
            return Json(new { success = result, redirectUrl = "/Lobby/Index" });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitKarma(string id, string targetUserId, double score)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.SubmitKarmaAsync(currentUserId, targetUserId, score);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> EditMission(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            };
            // var currentUserId = HttpContext.Session.GetString("UserId");
            // if (string.IsNullOrEmpty(currentUserId))
            //     return RedirectToAction("Login", "Auth");

            var lobby = await _lobbyService.GetLobbyByIdAsync(id);
            if (lobby == null) return NotFound();

            if (lobby.HostId != currentUserId)
                return Forbid();

            var editDto = new EditLobbyDTO
            {
                Id = lobby.Id,
                Title = lobby.Title,
                Game = lobby.Game,
                Description = lobby.Description,
                Picture = lobby.Picture,
                DiscordLink = lobby.DiscordLink,
                Moods = lobby.Moods,
                Roles = lobby.Roles,
                MaxPlayers = lobby.MaxPlayers,
                StartEvent = lobby.StartEvent,
                EndEvent = lobby.EndEvent,
                StartRecruiting = lobby.StartRecruiting,
                EndRecruiting = lobby.EndRecruiting,
                // Roles that are currently held by active members (locked from deletion/rename)
                OccupiedRoles = lobby.Members
                    .Where(m => m.Status != "Pending" && !string.IsNullOrEmpty(m.Role))
                    .Select(m => m.Role)
                    .Distinct()
                    .ToList()
            };

            return View("EditLobby", editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMission(EditLobbyDTO model)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var lobby = await _lobbyService.GetLobbyByIdAsync(model.Id);
            if (lobby == null) return NotFound();

            if (lobby.HostId != currentUserId)
                return Forbid();

            model.ApplyTo(lobby);
            await _lobbyService.UpdateLobbyAsync(lobby);

            return RedirectToAction("Details", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult ToggleRecruitment(string id)
        {
            return Json(new { success = true });
        }


        [HttpGet]
            public async Task<IActionResult> Create_lobby()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var user = await _mongoDBservice.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            var userName = user.Name;
            Console.WriteLine(userName);
            
            var model = new CreateLobbyDTO();
            model.HostId = userId;
            model.HostName = userName;
            model.StartRecruiting = DateTime.Now;
            model.EndRecruiting = DateTime.Now.AddDays(1); // ตัวอย่าง: ให้สิ้นสุดพรุ่งนี้
            model.StartEvent = DateTime.Now.AddDays(2);
            model.EndEvent = DateTime.Now.AddDays(2).AddHours(3);

            return View(model);  
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_lobby(CreateLobbyDTO model)
        { 
            // Console.Write(model);
            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is Valid");
                var (success, message) = await _lobbyService.CreateLobbyAsync(model);
                if (!success)
                {
                    // ใช้ ModelState เพิ่ม Error แทนการส่ง string เข้า View ตรงๆ
                    ModelState.AddModelError(string.Empty, message);
                    return View(model); 
                }
                return RedirectToAction("Index","Home");
            }
            Console.Write(model);

            return View(model);
        }

        // [HttpPost]
        // // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create_lobby(CreateLobbyDTO model)
        // {   
        //     Console.Write("Call create func");
        //     if (ModelState.IsValid)
        //     {
        //         string modelJson = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
        //         Console.WriteLine("--- Incoming Model Data ---");
        //         Console.WriteLine(modelJson);
                
        //         var (success,message) = await _lobbyService.CreateLobbyAsync(model);
        //         if (!success)
        //         {
        //             return View(message);
        //         }
        //         return RedirectToAction("Index");
        //     }
        //     return View(model);
        // }
       
    }
}