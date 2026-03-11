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
        

        public string HostId { get; set; } = null!;

        public int MaxPlayers { get; set; }
        public DateTime? StartRecruiting { get; set; }
    }
}