using System.Collections.Generic;
using System.Threading.Tasks;
using GamerLFG.Models;

namespace GamerLFG.Services
{
    public interface IFriendRequestService
    {

        Task<bool> SendRequestAsync(string senderId, string receiverId);
        

        Task<bool> AcceptRequestAsync(string requestId);

        Task<bool> RejectRequestAsync(string requestId);
        

        Task<List<FriendRequest>> GetPendingRequestsAsync(string userId);

        Task<bool> CancelRequestAsync(string senderId, string receiverId);

        Task<bool> HasPendingRequestAsync(string senderId, string receiverId);
    }
}