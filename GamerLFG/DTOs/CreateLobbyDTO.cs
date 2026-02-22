using System.ComponentModel.DataAnnotations;
using System;

namespace GamerLFG.DTOs
{
    public class CreateLobbyDTO
    {
        [Required(ErrorMessage = "กรุณาระบุชื่อปาร์ตี้")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "กรุณาระบุชื่อเกม")]
        public string Game { get; set; } = null!;

        public string? Description { get; set; }
        
        // [Required]
        public string HostId { get; set; } = null!; // รับมาจาก Session ของ User ที่ล็อกอิน

        public int MaxPlayers { get; set; }
        public DateTime? StartRecruiting { get; set; }
    }
}