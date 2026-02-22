using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ILobbyService _lobbyService;
        private readonly IUserService _user;

        public LobbyController(ILobbyService lobbyService, IUserService userService)
        {
            _lobbyService = lobbyService;
            _userService = userService;
        }

        // GET: /Lobby
        public async Task<IActionResult> Index()
        {
            var lobbies = await _lobbyService.GetLobbiesAsync();

            var userId = HttpContext.Session.GetString("UserId");
            var pendingLobbies = new List<Lobby>();

            if (userId != null)
            {
                var userApps = await _lobbyService.GetApplicationsByUserIdAsync(userId);
                var pendingLobbyIds = userApps.Where(a => a.Status == "Pending").Select(a => a.LobbyId).ToHashSet();
                pendingLobbies = lobbies.Where(l => pendingLobbyIds.Contains(l.Id)).ToList();
            }

            ViewData["PendingLobbies"] = pendingLobbies;

            // Pre-fetch Host names to avoid N+1 in View
            var hostIds = lobbies.Select(l => l.HostId).Distinct().ToList();
            var hosts = await _userService.GetUsersAsync(hostIds);
            ViewData["HostMap"] = hosts.ToDictionary(u => u.Id, u => u.Username);

            var joinedLobbies = new List<Lobby>();
            if (userId != null)
                joinedLobbies = lobbies.Where(l => l.Members.Any(m => m.UserId == userId && !m.IsHost)).ToList();
            ViewData["JoinedLobbies"] = joinedLobbies;

            var createdLobbies = new List<Lobby>();
            if (userId != null)
                createdLobbies = lobbies.Where(l => l.HostId == userId).ToList();
            ViewData["CreatedLobbies"] = createdLobbies;

            var myLobbyIds = new HashSet<string>(pendingLobbies.Select(l => l.Id));
            myLobbyIds.UnionWith(joinedLobbies.Select(l => l.Id));
            myLobbyIds.UnionWith(createdLobbies.Select(l => l.Id));

            var discoveryLobbies = lobbies.Where(l => !myLobbyIds.Contains(l.Id)).ToList();
            return View(discoveryLobbies);
        }

        // GET: /Lobby/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: /Lobby/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lobby lobby, List<string> moods, List<string> roleNames, List<int> roleCounts, string hostRole)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (ModelState.IsValid)
            {
                await _lobbyService.CreateLobbyAsync(userId, lobby, moods, roleNames, roleCounts, hostRole);
                TempData["Success"] = "Lobby created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.SubmittedMoods = moods ?? new List<string>();
            ViewBag.SubmittedRoleNames = roleNames ?? new List<string>();
            ViewBag.SubmittedRoleCounts = roleCounts ?? new List<int>();
            ViewBag.SubmittedHostRole = hostRole;
            return View(lobby);
        }

        // GET: /Lobby/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var lobby = await _lobbyService.GetLobbyAsync(id);
            if (lobby == null) return NotFound();
            if (lobby.HostId != userId) return Forbid();

            return View(lobby);
        }

        // POST: /Lobby/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Lobby updatedLobby, List<string> moods, List<string> roleNames, List<int> roleCounts)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var existingLobby = await _lobbyService.GetLobbyAsync(id);
            if (existingLobby == null) return NotFound();
            if (existingLobby.HostId != userId) return Forbid();

            await _lobbyService.UpdateLobbyAsync(id, userId, updatedLobby, moods, roleNames, roleCounts);
            TempData["Success"] = "Lobby updated successfully!";
            return RedirectToAction("Details", new { id });
        }

        // GET: /Lobby/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var lobby = await _lobbyService.GetLobbyAsync(id);
            if (lobby == null) return NotFound();

            if (lobby.Roles != null)
            {
                foreach (var role in lobby.Roles)
                    role.Filled = lobby.Members.Count(m => m.AssignedRole == role.Name);
            }

            var host = await _userService.GetUserAsync(lobby.HostId);
            ViewData["HostName"] = host?.Username ?? "Unknown";

            var memberIds = lobby.Members.Select(m => m.UserId).ToList();
            var members = await _userService.GetUsersAsync(memberIds);
            ViewData["MemberMap"] = members.ToDictionary(u => u.Id, u => u);

            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == lobby.HostId)
            {
                var applications = await _lobbyService.GetApplicationsByLobbyIdAsync(id);
                var pendingApplications = applications.Where(a => a.Status == "Pending").ToList();
                ViewData["PendingApplications"] = pendingApplications;

                var applicantIds = pendingApplications.Select(a => a.UserId).ToList();
                var applicants = await _userService.GetUsersAsync(applicantIds);
                ViewData["ApplicantMap"] = applicants.ToDictionary(u => u.Id, u => u);
            }

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var isMember = lobby.Members.Any(m => m.UserId == currentUserId);
                ViewData["IsMember"] = isMember;

                if (!isMember)
                {
                    var allApps = await _lobbyService.GetApplicationsByLobbyIdAsync(id);
                    var pendingApp = allApps.FirstOrDefault(a => a.UserId == currentUserId && a.Status == "Pending");
                    ViewData["PendingApplication"] = pendingApp;
                }
                else if (lobby.IsCompleted)
                {
                    var endorsements = await _userService.GetEndorsementsFromUserInLobbyAsync(currentUserId, id);
                    ViewData["EndorsedUserIds"] = endorsements.Select(e => e.ToUserId).ToHashSet();
                }
            }

            return View(lobby);
        }

        // GET: /Lobby/GetLobbies
        [HttpGet]
        public async Task<IActionResult> GetLobbies(string? game = null, string? mood = null, string? status = null, int page = 1)
        {
            var lobbies = await _lobbyService.GetLobbiesAsync(game);
            return Json(new { success = true, data = new { lobbies, pagination = new { currentPage = page, totalPages = 1 } } });
        }

        // POST: /Lobby/Apply/{id}
        [HttpPost]
        public async Task<IActionResult> Apply(string id, string? role = null)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            var (success, error) = await _lobbyService.ApplyAsync(id, userId, role);
            if (!success) return Json(new { success = false, error });
            return Json(new { success = true, message = "Application sent" });
        }

        // DELETE: /Lobby/CancelApplication/{id}
        [HttpDelete]
        public async Task<IActionResult> CancelApplication(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            await _lobbyService.CancelApplicationAsync(id, userId);
            return Json(new { success = true, message = "Action completed" });
        }

        // POST: /Lobby/Recruit/{id}
        [HttpPost]
        public async Task<IActionResult> Recruit(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            var (success, error) = await _lobbyService.RecruitAsync(id, userId);
            if (!success) return Json(new { success = false, error });
            return Json(new { success = true });
        }

        // POST: /Lobby/Reject/{id}
        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            var (success, error) = await _lobbyService.RejectAsync(id, userId);
            if (!success) return Json(new { success = false, error });
            return Json(new { success = true });
        }

        // POST: /Lobby/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var lobby = await _lobbyService.GetLobbyAsync(id);
            if (lobby == null) return NotFound();
            if (lobby.HostId != userId) return Forbid();

            await _lobbyService.DeleteLobbyAsync(id);
            return RedirectToAction("Index");
        }

        // POST: /Lobby/Kick
        [HttpPost]
        public async Task<IActionResult> Kick(string lobbyId, string memberId, bool hardKick = false)
        {
            var hostId = HttpContext.Session.GetString("UserId");
            if (hostId == null) return Json(new { success = false, error = "Please log in first" });

            var (success, error) = await _lobbyService.KickAsync(lobbyId, memberId, hostId, hardKick);
            if (!success) return Json(new { success = false, error });
            return Json(new { success = true, hardKick });
        }

        public class BackgroundUpdateRequest
        {
            public string PictureUrl { get; set; } = string.Empty;
        }

        // POST: /Lobby/UpdateBackground/{id}
        [HttpPost]
        public async Task<IActionResult> UpdateBackground(string id, [FromBody] BackgroundUpdateRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            var (success, error) = await _lobbyService.UpdateBackgroundAsync(id, request.PictureUrl, userId);
            if (!success) return Json(new { success = false, error });
            return Json(new { success = true });
        }

        // POST: /Lobby/ToggleRecruitment/{id}
        [HttpPost]
        public async Task<IActionResult> ToggleRecruitment(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, message = "Unauthorized" });

            var (success, isRecruiting, error) = await _lobbyService.ToggleRecruitmentAsync(id, userId);
            if (!success) return Json(new { success = false, message = error });
            return Json(new { success = true, isRecruiting });
        }

        // POST: /Lobby/CompleteSession/{id}
        [HttpPost]
        public async Task<IActionResult> CompleteSession(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, message = "Unauthorized" });

            var (success, error) = await _lobbyService.CompleteSessionAsync(id, userId);
            if (!success) return Json(new { success = false, message = error });
            return Json(new { success = true });
        }

        // POST: /Lobby/EndorseMember
        [HttpPost]
        public async Task<IActionResult> EndorseMember(string lobbyId, string targetUserId, string type)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "Not logged in" });

            var (success, error) = await _lobbyService.EndorseMemberAsync(lobbyId, userId, targetUserId, type);
            if (!success) return Json(new { success = false, message = error });
            return Json(new { success = true });
        }
    }
}
