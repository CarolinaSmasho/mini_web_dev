using System.Collections.Generic;
using System.Threading.Tasks;
using GamerLFG.Models;

namespace GamerLFG.Services
{
    public interface IFriendRequestService
    {
        // 1. ส่งคำขอเป็นเพื่อน
        Task<bool> SendRequestAsync(string senderId, string receiverId);
        
        // 2. กดยอมรับคำขอ (ต้องดึง ID ของ Request มาทำงาน)
        Task<bool> AcceptRequestAsync(string requestId);

        Task<bool> RejectRequestAsync(string requestId);
        
        // 3. ดึงรายการคำขอที่รอการยืนยัน (เอาไว้โชว์หน้ากระดิ่งแจ้งเตือน)
        Task<List<FriendRequest>> GetPendingRequestsAsync(string userId);
    }
}