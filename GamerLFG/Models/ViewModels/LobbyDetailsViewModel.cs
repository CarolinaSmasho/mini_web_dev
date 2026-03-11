using GamerLFG.Models;

namespace GamerLFG.Models.ViewModels
{

    public class LobbyDetailsViewModel
    {

        public Lobby Lobby { get; set; } = new();

        public string? CurrentUserId { get; set; }

        public User? CurrentUser { get; set; }

        public string CurrentUsername =>
            CurrentUser?.Username ?? CurrentUserId ?? "Not logged in";

        public bool IsHost { get; set; }

        public bool IsMember { get; set; }
        public bool HasPendingRequest { get; set; }

        public Dictionary<string, User> MemberMap { get; set; } = new();

        public List<LobbyMember> PendingApplications { get; set; } = new();

        public Dictionary<string, User> ApplicantMap { get; set; } = new();

        public HashSet<string> EndorsedUserIds { get; set; } = new();

        public List<User> InvitableFriends { get; set; } = new();

        public bool HasPendingInvite { get; set; }

        public string? InvitedByName { get; set; }

        public List<string> PlayerIds =>
            Lobby.Members
                 .Where(m => m.Status != "Pending")
                 .Select(m => m.UserId)
                 .ToList();

        public string HostName =>
            MemberMap.TryGetValue(Lobby.HostId ?? "", out var h)
                ? h.Username
                : (Lobby.HostId ?? "Unknown");
    }
}
