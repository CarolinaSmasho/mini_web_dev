using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Models.TestData;
using GamerLFG.Services;
using GamerLFG.Services.Interface.DTOs;
using System.Text.Json;
using GamerLFG.Services.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using GamerLFG.service;
using MongoDB.Driver;
namespace GamerLFG.Controllers 

{   

    public class LobbyController : Controller
    {
        private readonly ILobbyService _lobbyService;
        private readonly MongoDBservice _mongoDBservice;
        public LobbyController(ILobbyService lobbyService,MongoDBservice mongoDBservice)
        {
            _lobbyService = lobbyService;
            _mongoDBservice = mongoDBservice;

        }
        public async Task<IActionResult> Details(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = await _lobbyService.GetLobbyDetailsAsync(id, currentUserId);
            if (viewModel == null) return NotFound();
            return View(viewModel);
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var lobbyData = await _lobbyService.GetAllLobbyAsync(userId);
            return View(lobbyData);
        }

        [HttpGet]
        public IActionResult SwitchUser(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, "Test User"),
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            var lobbyId = LobbySeeder.IdLobby;
            return Redirect($"/Lobby/Details/{lobbyId}");
        }

        

        [HttpPost]
        public async Task<IActionResult> Apply(string id, string role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var (success, message) = await _lobbyService.ApplyToLobbyAsync(id, currentUserId, role);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.CancelApplicationAsync(id, currentUserId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Recruit(string id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.RecruitMemberAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.RejectApplicantAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> Kick(string id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.KickMemberAsync(id, userId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> AbandonMission(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.KickMemberAsync(id, currentUserId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMission(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.CompleteLobbyAsync(id);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> TerminateLobby(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.DeleteLobbyAsync(id);
            return Json(new { success = result, redirectUrl = "/" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string id, string userId, string newRole)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var lobby = await _lobbyService.GetLobbyByIdAsync(id);
            if (lobby == null) return NotFound();

            bool isHost = lobby.HostId == currentUserId;
            bool isSelf = userId == currentUserId;

            if (!isHost && !isSelf)
                return Forbid();

            var result = await _lobbyService.ChangeMemberRoleAsync(id, userId, newRole);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitKarma(string id, string targetUserId, double score)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var result = await _lobbyService.SubmitKarmaAsync(id, currentUserId, targetUserId, score);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> EditMission(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return RedirectToAction("Login", "Auth");

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

                OccupiedRoles = lobby.Members
                    .Where(m => !string.IsNullOrEmpty(m.Role))
                    .Select(m => m.Role)
                    .Distinct()
                    .ToList(),

                OccupiedRoleCounts = lobby.Members
                    .Where(m => !string.IsNullOrEmpty(m.Role))
                    .GroupBy(m => m.Role)
                    .ToDictionary(g => g.Key, g => g.Count()),

                CurrentMemberCount = lobby.Members.Count(m => m.Status != "Pending")
            };

            return View("EditLobby", editDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMission(EditLobbyDTO model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        public async Task<IActionResult> InviteFriend(string id, string friendId, string role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var (success, message) = await _lobbyService.InviteFriendAsync(id, currentUserId, friendId, role);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvite(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var (success, message) = await _lobbyService.AcceptInviteAsync(id, currentUserId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> DeclineInvite(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var (success, message) = await _lobbyService.DeclineInviteAsync(id, currentUserId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRecruitment(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { success = false, message = "Not logged in" });

            var lobby = await _lobbyService.GetLobbyByIdAsync(id);
            if (lobby == null) return NotFound();

            if (lobby.HostId != currentUserId)
                return Forbid();

            var result = await _lobbyService.ToggleRecruitmentAsync(id);
            return Json(new { success = result, isRecruiting = !lobby.IsRecruiting });
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
            model.EndRecruiting = DateTime.Now.AddDays(1);
            model.StartEvent = DateTime.Now.AddDays(2);
            model.EndEvent = DateTime.Now.AddDays(2).AddHours(3);

            return View(model);  
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_lobby(CreateLobbyDTO model)
        {

            if (ModelState.IsValid)
            {
                Console.WriteLine("Model is Valid");
                var (success, message) = await _lobbyService.CreateLobbyAsync(model);
                if (!success)
                {

                    ModelState.AddModelError(string.Empty, message);
                    return View(model);
                }
                return RedirectToAction("Index", "Home");
            }
            Console.Write(model);

            return View(model);
        }

    }
}