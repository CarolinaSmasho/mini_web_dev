using GamerLFG.Models;
using GamerLFG.Repositories;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public class KarmaService
    {
        private readonly IKarmaRepository _karmaRepository;
        private readonly IUserRepository _userRepository;

        public KarmaService(IKarmaRepository karmaRepository, IUserRepository userRepository)
        {
            _karmaRepository = karmaRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Award karma points to a user and update their total karma score
        /// </summary>
        public async Task AwardKarmaAsync(
            string userId, 
            string actionType, 
            int points, 
            string? referenceType = null, 
            string? referenceId = null, 
            string? description = null)
        {
            // Create karma history entry
            var entry = new KarmaHistory
            {
                UserId = userId,
                ActionType = actionType,
                Points = points,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Description = description
            };

            await _karmaRepository.CreateAsync(entry);

            // Update user's karma score
            var user = await _userRepository.GetUserAsync(userId);
            if (user != null)
            {
                user.KarmaScore += points;
                await _userRepository.UpdateUserAsync(user);
            }
        }

        /// <summary>
        /// Award karma for being accepted into a lobby
        /// </summary>
        public async Task AwardLobbyAcceptedAsync(string userId, string lobbyId, string lobbyTitle)
        {
            await AwardKarmaAsync(
                userId,
                KarmaActionTypes.LOBBY_ACCEPTED,
                KarmaPoints.LOBBY_ACCEPTED,
                "Lobby",
                lobbyId,
                $"Accepted into lobby: {lobbyTitle}"
            );
        }

        /// <summary>
        /// Award karma for completing a lobby as host
        /// </summary>
        public async Task AwardLobbyCompletedHostAsync(string userId, string lobbyId, string lobbyTitle)
        {
            await AwardKarmaAsync(
                userId,
                KarmaActionTypes.LOBBY_COMPLETED_HOST,
                KarmaPoints.LOBBY_COMPLETED_HOST,
                "Lobby",
                lobbyId,
                $"Hosted completed lobby: {lobbyTitle}"
            );
        }

        /// <summary>
        /// Award karma for completing a lobby as member
        /// </summary>
        public async Task AwardLobbyCompletedMemberAsync(string userId, string lobbyId, string lobbyTitle)
        {
            await AwardKarmaAsync(
                userId,
                KarmaActionTypes.LOBBY_COMPLETED_MEMBER,
                KarmaPoints.LOBBY_COMPLETED_MEMBER,
                "Lobby",
                lobbyId,
                $"Completed lobby: {lobbyTitle}"
            );
        }

        /// <summary>
        /// Penalize karma for being kicked from lobby (hard kick only)
        /// </summary>
        public async Task PenalizeKickedAsync(string userId, string lobbyId, string lobbyTitle, bool hardKick)
        {
            var actionType = hardKick ? KarmaActionTypes.LOBBY_HARD_KICKED : KarmaActionTypes.LOBBY_SOFT_KICKED;
            var points = hardKick ? KarmaPoints.LOBBY_HARD_KICKED : KarmaPoints.LOBBY_SOFT_KICKED;
            var desc = hardKick 
                ? $"Hard kicked from lobby: {lobbyTitle}" 
                : $"Soft kicked from lobby: {lobbyTitle}";

            await AwardKarmaAsync(
                userId,
                actionType,
                points,
                "Lobby",
                lobbyId,
                desc
            );
        }

        /// <summary>
        /// Penalize karma for leaving lobby early
        /// </summary>
        public async Task PenalizeLeftEarlyAsync(string userId, string lobbyId, string lobbyTitle)
        {
            await AwardKarmaAsync(
                userId,
                KarmaActionTypes.LOBBY_LEFT_EARLY,
                KarmaPoints.LOBBY_LEFT_EARLY,
                "Lobby",
                lobbyId,
                $"Left lobby early: {lobbyTitle}"
            );
        }

        /// <summary>
        /// Award/penalize karma for endorsement
        /// </summary>
        public async Task ProcessEndorsementAsync(string toUserId, string fromUsername, string endorsementType, string endorsementId)
        {
            bool isPositive = IsPositiveEndorsement(endorsementType);
            var actionType = isPositive ? KarmaActionTypes.ENDORSEMENT_POSITIVE : KarmaActionTypes.ENDORSEMENT_NEGATIVE;
            var points = isPositive ? KarmaPoints.ENDORSEMENT_POSITIVE : KarmaPoints.ENDORSEMENT_NEGATIVE;

            await AwardKarmaAsync(
                toUserId,
                actionType,
                points,
                "Endorsement",
                endorsementId,
                $"Received {endorsementType} endorsement from {fromUsername}"
            );
        }

        /// <summary>
        /// Check if endorsement type is positive
        /// </summary>
        private bool IsPositiveEndorsement(string endorsementType)
        {
            var positiveTypes = new[] { "helpful", "skilled", "friendly", "good_teammate", "chill", "leader", "positive" };
            return positiveTypes.Any(t => endorsementType.ToLower().Contains(t));
        }

        /// <summary>
        /// Get karma history for a user (public access)
        /// </summary>
        public async Task<List<KarmaHistory>> GetKarmaHistoryAsync(string userId, int limit = 50)
        {
            return await _karmaRepository.GetByUserIdAsync(userId, limit);
        }

        /// <summary>
        /// Recalculate user's karma from history
        /// </summary>
        public async Task RecalculateKarmaAsync(string userId)
        {
            var totalPoints = await _karmaRepository.GetTotalPointsByUserIdAsync(userId);
            var user = await _userRepository.GetUserAsync(userId);
            if (user != null)
            {
                user.KarmaScore = totalPoints;
                await _userRepository.UpdateUserAsync(user);
            }
        }
    }
}
