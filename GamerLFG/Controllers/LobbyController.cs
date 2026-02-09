using Microsoft.AspNetCore.Mvc;

using GamerLFG.Models;
using GamerLFG.Repositories;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly KarmaService _karmaService;
        private readonly NotificationService _notificationService;

        public LobbyController(ILobbyRepository lobbyRepository, IUserRepository userRepository, 
            IApplicationRepository applicationRepository, IEndorsementRepository endorsementRepository,
            KarmaService karmaService, NotificationService notificationService)
        {
            _lobbyRepository = lobbyRepository;
            _userRepository = userRepository;
            _applicationRepository = applicationRepository;
            _endorsementRepository = endorsementRepository;
            _karmaService = karmaService;
            _notificationService = notificationService;
        }

        // GET: /Lobby
        public async Task<IActionResult> Index()
        {
            var lobbies = await _lobbyRepository.GetLobbiesAsync();
            
            // Pending Lobbies Logic
            var userId = HttpContext.Session.GetString("UserId");
            var pendingLobbies = new List<Lobby>();

            if (userId != null)
            {
                var applications = await _applicationRepository.GetApplicationsByUserIdAsync(userId);
                var pendingLobbyIds = applications
                    .Where(a => a.Status == "Pending")
                    .Select(a => a.LobbyId)
                    .ToHashSet();
                
                pendingLobbies = lobbies.Where(l => pendingLobbyIds.Contains(l.Id)).ToList();
            }

            ViewData["PendingLobbies"] = pendingLobbies;

            // Pre-fetch Host names to avoid N+1 in View
            var hostIds = lobbies.Select(l => l.HostId).Distinct().ToList();
            var hosts = await _userRepository.GetUsersAsync(hostIds);
            var hostMap = hosts.ToDictionary(u => u.Id, u => u.Username);
            
            ViewData["HostMap"] = hostMap;

            // Joined Lobbies (Member but not Host)
            var joinedLobbies = new List<Lobby>();
            if (userId != null)
            {
                joinedLobbies = lobbies.Where(l => l.Members.Any(m => m.UserId == userId && !m.IsHost)).ToList();
            }
            ViewData["JoinedLobbies"] = joinedLobbies;

            // Created Lobbies (Host)
            var createdLobbies = new List<Lobby>();
            if (userId != null)
            {
                createdLobbies = lobbies.Where(l => l.HostId == userId).ToList();
            }
            ViewData["CreatedLobbies"] = createdLobbies;

            // Filter out lobbies that are already in "My Lobbies" sections
            var myLobbyIds = new HashSet<string>(pendingLobbies.Select(l => l.Id));
            myLobbyIds.UnionWith(joinedLobbies.Select(l => l.Id));
            myLobbyIds.UnionWith(createdLobbies.Select(l => l.Id));

            var discoveryLobbies = lobbies.Where(l => !myLobbyIds.Contains(l.Id)).ToList();

            return View(discoveryLobbies);
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
        public async Task<IActionResult> Create(Lobby lobby, List<string> moods, List<string> roleNames, List<int> roleCounts, string hostRole)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Manually bind the lists if they come in separately or process them
            // The model binder should handle 'lobby' properties if names match, but complex lists might need help
            // Re-map Roles manually from arrays
            lobby.Roles = new List<Role>();
            int totalRolePlayers = 0;
            if (roleNames != null && roleCounts != null)
            {
                for (int i = 0; i < roleNames.Count; i++)
                {
                    if (i < roleCounts.Count && !string.IsNullOrWhiteSpace(roleNames[i]))
                    {
                        int count = roleCounts[i];
                        // If adding this would exceed MaxPlayers, cap it
                        if (totalRolePlayers + count > lobby.MaxPlayers)
                        {
                            count = lobby.MaxPlayers - totalRolePlayers;
                        }

                        if (count > 0)
                        {
                            lobby.Roles.Add(new Role 
                            { 
                                Name = roleNames[i], 
                                Count = count,
                                Filled = 0 
                            });
                            totalRolePlayers += count;
                        }
                    }
                    if (totalRolePlayers >= lobby.MaxPlayers) break;
                }
            }

            // If total roles < max players, add "Other" role for the remainder
            if (totalRolePlayers < lobby.MaxPlayers)
            {
                lobby.Roles.Add(new Role
                {
                    Name = "Other",
                    Count = lobby.MaxPlayers - totalRolePlayers,
                    Filled = 0
                });
            }
            
            lobby.Moods = moods ?? new List<string>();

            if (ModelState.IsValid)
            {
                lobby.HostId = userId;
                
                // Determine host role
                var assignedRole = !string.IsNullOrEmpty(hostRole) 
                    ? hostRole 
                    : (lobby.Roles.FirstOrDefault()?.Name ?? "Host");
                
                // Add Host as a Member
                lobby.Members = new List<Member>
                {
                    new Member 
                    { 
                        UserId = userId, 
                        AssignedRole = assignedRole, 
                        IsHost = true, 
                        JoinedAt = DateTime.UtcNow 
                    }
                };
                
                // Update Filled count for the role assigned to host
                var assignedHostRole = lobby.Roles.FirstOrDefault(r => r.Name == lobby.Members[0].AssignedRole);
                if (assignedHostRole != null) assignedHostRole.Filled = 1;
                
                if (lobby.RecruitmentDeadline.HasValue) 
                    lobby.RecruitmentDeadline = DateTime.SpecifyKind(lobby.RecruitmentDeadline.Value, DateTimeKind.Unspecified).ToUniversalTime();
                if (lobby.SessionStartTime.HasValue)
                    lobby.SessionStartTime = DateTime.SpecifyKind(lobby.SessionStartTime.Value, DateTimeKind.Unspecified).ToUniversalTime();
                if (lobby.SessionEndTime.HasValue)
                    lobby.SessionEndTime = DateTime.SpecifyKind(lobby.SessionEndTime.Value, DateTimeKind.Unspecified).ToUniversalTime();

                lobby.CreatedAt = DateTime.UtcNow;
                
                await _lobbyRepository.CreateLobbyAsync(lobby);

                TempData["Success"] = "Lobby created successfully!";
                return RedirectToAction("Index");
            }

            return View(lobby);
        }

        // GET: /Lobby/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
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

            var existingLobby = await _lobbyRepository.GetLobbyAsync(id);
            if (existingLobby == null) return NotFound();

            if (existingLobby.HostId != userId) return Forbid();

            // Manually bind Roles
            existingLobby.Roles = new List<Role>();
            int totalEditRolePlayers = 0;
            if (roleNames != null && roleCounts != null)
            {
                for (int i = 0; i < roleNames.Count; i++)
                {
                    if (i < roleCounts.Count && !string.IsNullOrWhiteSpace(roleNames[i]))
                    {
                        int count = roleCounts[i];
                        if (totalEditRolePlayers + count > updatedLobby.MaxPlayers)
                        {
                            count = updatedLobby.MaxPlayers - totalEditRolePlayers;
                        }

                        if (count > 0)
                        {
                            existingLobby.Roles.Add(new Role 
                            { 
                                Name = roleNames[i], 
                                Count = count,
                                Filled = 0
                            });
                            totalEditRolePlayers += count;
                        }
                    }
                    if (totalEditRolePlayers >= updatedLobby.MaxPlayers) break;
                }
            }

            // Add "Other" if needed
            if (totalEditRolePlayers < updatedLobby.MaxPlayers)
            {
                existingLobby.Roles.Add(new Role
                {
                    Name = "Other",
                    Count = updatedLobby.MaxPlayers - totalEditRolePlayers,
                    Filled = 0
                });
            }
            
            // Recalculate Filled roles based on existing members
            foreach(var role in existingLobby.Roles)
            {
                role.Filled = existingLobby.Members.Count(m => m.AssignedRole == role.Name);
            }

            existingLobby.Title = updatedLobby.Title;
            existingLobby.Game = updatedLobby.Game;
            existingLobby.Description = updatedLobby.Description;
            existingLobby.MaxPlayers = updatedLobby.MaxPlayers;
            existingLobby.PictureUrl = updatedLobby.PictureUrl;
            existingLobby.DiscordLink = updatedLobby.DiscordLink;
            
            if (updatedLobby.RecruitmentDeadline.HasValue)
                existingLobby.RecruitmentDeadline = DateTime.SpecifyKind(updatedLobby.RecruitmentDeadline.Value, DateTimeKind.Unspecified).ToUniversalTime();
            if (updatedLobby.SessionStartTime.HasValue)
                existingLobby.SessionStartTime = DateTime.SpecifyKind(updatedLobby.SessionStartTime.Value, DateTimeKind.Unspecified).ToUniversalTime();
            if (updatedLobby.SessionEndTime.HasValue)
                existingLobby.SessionEndTime = DateTime.SpecifyKind(updatedLobby.SessionEndTime.Value, DateTimeKind.Unspecified).ToUniversalTime();

            existingLobby.Moods = moods ?? new List<string>();

            await _lobbyRepository.UpdateLobbyAsync(existingLobby);

            TempData["Success"] = "Lobby updated successfully!";
            return RedirectToAction("Details", new { id = existingLobby.Id });
        }

        // GET: /Lobby/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound();

            // Recalculate Filled roles based on current members
            if (lobby.Roles != null)
            {
                foreach(var role in lobby.Roles)
                {
                    role.Filled = lobby.Members.Count(m => m.AssignedRole == role.Name);
                }
            }

            var host = await _userRepository.GetUserAsync(lobby.HostId);
            ViewData["HostName"] = host?.Username ?? "Unknown";

            // Fetch all members details for the view
            var memberIds = lobby.Members.Select(m => m.UserId).ToList();
            var members = await _userRepository.GetUsersAsync(memberIds);
            
            // Map users to a dictionary for easy lookup in View
            ViewData["MemberMap"] = members.ToDictionary(u => u.Id, u => u);

            // Host specific data
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == lobby.HostId)
            {
                var applications = await _applicationRepository.GetApplicationsByLobbyIdAsync(id);
                var pendingApplications = applications.Where(a => a.Status == "Pending").ToList();
                ViewData["PendingApplications"] = pendingApplications;
                
                var applicantIds = pendingApplications.Select(a => a.UserId).ToList();
                var applicants = await _userRepository.GetUsersAsync(applicantIds);
                ViewData["ApplicantMap"] = applicants.ToDictionary(u => u.Id, u => u);
            }
            
            // Current User status for the view
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var isMember = lobby.Members.Any(m => m.UserId == currentUserId);
                ViewData["IsMember"] = isMember;
                
                if (!isMember)
                {
                    var userApps = await _applicationRepository.GetApplicationsByUserIdAsync(currentUserId);
                    var pendingApp = userApps.FirstOrDefault(a => a.LobbyId == id && a.Status == "Pending");
                    ViewData["PendingApplication"] = pendingApp;
                }
                else if (lobby.IsCompleted)
                {
                    // Check for existing endorsements
                    var endorsements = await _endorsementRepository.GetEndorsementsFromUserInLobbyAsync(currentUserId, id);
                    var endorsedUserIds = endorsements.Select(e => e.ToUserId).ToHashSet();
                    ViewData["EndorsedUserIds"] = endorsedUserIds;
                }
            }

            return View(lobby);
        }

        // API Endpoints for Ajax

        // GET: /Lobby/GetLobbies
        [HttpGet]
        public async Task<IActionResult> GetLobbies(string? game = null, string? mood = null, string? status = null, int page = 1)
        {
            var lobbies = await _lobbyRepository.GetLobbiesAsync();
            
            if (!string.IsNullOrEmpty(game))
            {
                lobbies = lobbies.Where(l => l.Game.Contains(game, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Json(new { success = true, data = new { lobbies, pagination = new { currentPage = page, totalPages = 1 } } });
        }

        // POST: /Lobby/Apply/{id}
        [HttpPost]
        public async Task<IActionResult> Apply(string id, string? role = null)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Json(new { success = false, error = "Please log in first" });
            }

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return Json(new { success = false, error = "Lobby not found" });

            if (lobby.RecruitmentDeadline.HasValue && DateTime.UtcNow > lobby.RecruitmentDeadline.Value)
            {
                return Json(new { success = false, error = "Recruitment period has ended for this lobby." });
            }

            // Check if already member
            if (lobby.Members.Any(m => m.UserId == userId))
            {
                 return Json(new { success = false, error = "You are already in this lobby" });
            }

            // Check if application exists
            var existingApps = await _applicationRepository.GetApplicationsByLobbyIdAsync(id);
            if (existingApps.Any(a => a.UserId == userId && a.Status == "Pending"))
            {
                return Json(new { success = false, error = "Application already pending" });
            }
            
            // Create Application
            var application = new Application
            {
                LobbyId = id,
                UserId = userId,
                DesiredRoles = !string.IsNullOrEmpty(role) ? new List<string> { role } : new List<string>(),
                Message = string.IsNullOrEmpty(role) ? "Requesting to join" : $"Applying for {role}",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };
            
            await _applicationRepository.CreateApplicationAsync(application);

            // Notify Host
            var user = await _userRepository.GetUserAsync(userId);
            await _notificationService.NotifyUserAsync(lobby.HostId, "Application", $"New application from {user?.Username ?? "Unknown"} for {lobby.Title}", application.Id);
            
            return Json(new { success = true, message = "Application sent" });
        }

        // DELETE: /Lobby/CancelApplication/{id} (Leave/Cancel)
        [HttpDelete]
        public async Task<IActionResult> CancelApplication(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return Json(new { success = false, error = "Please log in first" });
            }

            // 'id' is the LobbyId
            await _lobbyRepository.RemoveMemberAsync(id, userId);
            
            // Find and delete any application for this user in this lobby
            var applications = await _applicationRepository.GetApplicationsByUserIdAsync(userId);
            var lobbyApp = applications.FirstOrDefault(a => a.LobbyId == id);
            if (lobbyApp != null)
            {
                await _applicationRepository.DeleteApplicationAsync(lobbyApp.Id);
            }
            
            return Json(new { success = true, message = "Action completed" });
        }

        // POST: /Lobby/Recruit/{id} (Application ID)
        [HttpPost]
        public async Task<IActionResult> Recruit(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            // 'id' here is the Application ID
            var application = await _applicationRepository.GetApplicationAsync(id);
            if (application == null) return Json(new { success = false, error = "Application not found" });

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return Json(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != userId) return Json(new { success = false, error = "Not authorized" });

            if (lobby.Members.Count >= lobby.MaxPlayers) return Json(new { success = false, error = "Lobby is full" });

            if (lobby.Members.Any(m => m.UserId == application.UserId)) return Json(new { success = false, error = "User is already a member" });

            // Update Application
            await _applicationRepository.UpdateApplicationStatusAsync(id, "Accepted");

            // Add Member
            var role = application.DesiredRoles?.FirstOrDefault() ?? "Member";
            var member = new Member
            {
                UserId = application.UserId,
                AssignedRole = role,
                IsHost = false,
                JoinedAt = DateTime.UtcNow
            };

            await _lobbyRepository.AddMemberAsync(lobby.Id, member);

            // Award karma to the new member
            await _karmaService.AwardLobbyAcceptedAsync(application.UserId, lobby.Id, lobby.Title);

            // Notify Applicant
            await _notificationService.NotifyUserAsync(application.UserId, "Recruitment", $"You have been recruited to {lobby.Title}!", lobby.Id);

            return Json(new { success = true });
        }

        // POST: /Lobby/Reject/{id} (Application ID)
        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Json(new { success = false, error = "Please log in first" });

            var application = await _applicationRepository.GetApplicationAsync(id);
            if (application == null) return Json(new { success = false, error = "Application not found" });

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return Json(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != userId) return Json(new { success = false, error = "Not authorized" });

            await _applicationRepository.UpdateApplicationStatusAsync(id, "Rejected");

            // Notify Applicant
            await _notificationService.NotifyUserAsync(application.UserId, "Recruitment", $"Your application to {lobby.Title} was declined.", lobby.Id);

            return Json(new { success = true });
        }

        // POST: /Lobby/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound();

            if (lobby.HostId != userId) return Forbid();

            await _lobbyRepository.DeleteLobbyAsync(id);
            await _applicationRepository.DeleteApplicationsByLobbyIdAsync(id);

            return RedirectToAction("Index");
        }

        // POST: /Lobby/Kick
        [HttpPost]
        public async Task<IActionResult> Kick(string lobbyId, string memberId, bool hardKick = false)
        {
            var hostId = HttpContext.Session.GetString("UserId");
            if (hostId == null) return Json(new { success = false, error = "Please log in first" });

            var lobby = await _lobbyRepository.GetLobbyAsync(lobbyId);
            if (lobby == null) return Json(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != hostId) return Json(new { success = false, error = "Not authorized" });
            if (memberId == lobby.HostId) return Json(new { success = false, error = "Cannot kick host" });

            await _lobbyRepository.RemoveMemberAsync(lobbyId, memberId);

            // Apply karma penalty (only for hard kick)
            await _karmaService.PenalizeKickedAsync(memberId, lobbyId, lobby.Title, hardKick);

            // Notify Member
            await _notificationService.NotifyUserAsync(memberId, "System", $"You were removed from lobby {lobby.Title}.", lobbyId);

            return Json(new { success = true, hardKick = hardKick });
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

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return Json(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != userId) return Json(new { success = false, error = "Not authorized" });

            lobby.PictureUrl = request.PictureUrl;
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            return Json(new { success = true });
        }

        // POST: /Lobby/ToggleRecruitment/{id}
        [HttpPost]
        public async Task<IActionResult> ToggleRecruitment(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != userId) return Json(new { success = false, message = "Unauthorized" });

            lobby.IsRecruiting = !lobby.IsRecruiting;
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            return Json(new { success = true, isRecruiting = lobby.IsRecruiting });
        }

        // POST: /Lobby/CompleteSession/{id}
        [HttpPost]
        public async Task<IActionResult> CompleteSession(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != userId) return Json(new { success = false, message = "Unauthorized" });

            lobby.IsCompleted = true;
            lobby.Status = "Closed";
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            // Award karma to host
            await _karmaService.AwardLobbyCompletedHostAsync(lobby.HostId, lobby.Id, lobby.Title);

            // Award karma to all members (except host)
            foreach (var member in lobby.Members.Where(m => !m.IsHost))
            {
                await _karmaService.AwardLobbyCompletedMemberAsync(member.UserId, lobby.Id, lobby.Title);
            }

            return Json(new { success = true });
        }

        // POST: /Lobby/EndorseMember
        [HttpPost]
        public async Task<IActionResult> EndorseMember(string lobbyId, string targetUserId, string type)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Json(new { success = false, message = "Not logged in" });

            var lobby = await _lobbyRepository.GetLobbyAsync(lobbyId);
            if (lobby == null || !lobby.IsCompleted) return Json(new { success = false, message = "Session not completed" });

            // Check if both are members
            if (!lobby.Members.Any(m => m.UserId == userId) || !lobby.Members.Any(m => m.UserId == targetUserId))
                return Json(new { success = false, message = "Invalid members" });

            // Check if already endorsed
            if (await _endorsementRepository.HasEndorsedInLobbyAsync(userId, targetUserId, lobbyId))
            {
                return Json(new { success = false, message = "You have already evaluated this member for this mission." });
            }

            var endorsement = new Endorsement
            {
                FromUserId = userId,
                ToUserId = targetUserId,
                EndorsementType = type, 
                Comment = "Session Feedback",
                LobbyId = lobbyId, 
                CreatedAt = DateTime.UtcNow
            };

            await _endorsementRepository.CreateEndorsementAsync(endorsement);

            // Use KarmaService for endorsement karma
            var fromUser = await _userRepository.GetUserAsync(userId);
            await _karmaService.ProcessEndorsementAsync(
                targetUserId, 
                fromUser?.Username ?? "Unknown", 
                type, 
                endorsement.Id
            );

            return Json(new { success = true });
        }
    }
}
