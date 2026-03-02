using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GamerLFG.Models;
namespace GamerLFG.Services.Interface.DTOs
{
    public class ShowLobbyDTO
    {
        public string Id { get; set; }
        public string Title {get;set;}
        public string Game { get; set; }
        public string Description { get; set; }
        public string HostName { get; set; }
        public string Picture { get; set; }
        public List<string> Moods {get;set;}
        public int Currentplayers {get;set;}
        public int MaxPlayers {get;set;}

    }

    public class CreateLobbyDTO
    {
        [Required(ErrorMessage = "กรุณาระบุชื่อห้อง")]
        [StringLength(100, ErrorMessage = "ชื่อห้องยาวเกินไป")]
        public string Title { get; set; } //
        
        [Required(ErrorMessage = "กรุณาระบุชื่อเกม")]
        public string Game { get; set; }

        public string Description { get; set; }//

        public string HostId {get;set;}
        public string Picture { get; set; } //

        [Required(ErrorMessage = "กรุณาใส่ลิงก์ Discord เพื่อใช้สื่อสาร")]
        [Url(ErrorMessage = "รูปแบบลิงก์ไม่ถูกต้อง")]
        public string DiscordLink { get; set; }

        // Tags สำหรับเลือกแนวการเล่น เช่น "Chill", "Serious"
        public List<string> Moods { get; set; } = new(); //

        // ตำแหน่งที่ต้องการ เช่น "Tank", "Healer"
        public List<string> Roles { get; set; } = new(); //
        public string HostRole { get; set; } = "GAY";

        [Required]
        [Range(2, 100, ErrorMessage = "จำนวนผู้เล่นต้องอยู่ระหว่าง 2 - 100 คน")]
        public int MaxPlayers { get; set; }
       
        // วันเวลาที่เกี่ยวข้อง
        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มกิจกรรม")]
        public DateTime StartEvent { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดกิจกรรม")]
        public DateTime EndEvent { get; set; }

        // หมายเหตุ: Start/End Recruiting อาจจะตั้งค่า Default 
        // หรือรับมาจากหน้าฟอร์มก็ได้ แล้วแต่การออกแบบ UI ของคุณ
        public DateTime StartRecruiting { get; set; }
        public DateTime EndRecruiting { get; set; }

        public Lobby ToEntity()
        {
            return new Lobby
            {
                
                Title = this.Title, //
                Game = this.Game, //
                Description = this.Description, // 
                Picture = this.Picture, //
                DiscordLink = this.DiscordLink, //
                Moods = this.Moods, //
                Roles = this.Roles, //
                MaxPlayers = this.MaxPlayers, //
                StartRecruiting = this.StartRecruiting, //
                EndRecruiting = this.EndRecruiting, //
                StartEvent = this.StartEvent, // 
                EndEvent = this.EndEvent, //
                Members = new List<GamerLFG.Models.LobbyMember> 
                { 
                    new LobbyMember 
                    { 
                        UserId = this.HostId, 
                        Status = "Host", 
                        Role = this.HostRole,
                    }
                    },
        };
        }

        

    };
public class LobbyListResponse
        {
            public List<ShowLobbyDTO> MyLobbies { get; set; } = new();
            public List<ShowLobbyDTO> OtherLobbies { get; set; } = new();
        }

    public class EditLobbyDTO
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "กรุณาระบุชื่อห้อง")]
        [StringLength(100, ErrorMessage = "ชื่อห้องยาวเกินไป")]
        public string Title { get; set; }

        [Required(ErrorMessage = "กรุณาระบุชื่อเกม")]
        public string Game { get; set; }

        public string Description { get; set; }
        public string Picture { get; set; }

        [Required(ErrorMessage = "กรุณาใส่ลิงก์ Discord เพื่อใช้สื่อสาร")]
        [Url(ErrorMessage = "รูปแบบลิงก์ไม่ถูกต้อง")]
        public string DiscordLink { get; set; }

        public List<string> Moods { get; set; } = new();
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// Role names that are currently held by at least one active member.
        /// NOT bound from form — populated server-side only.
        /// Sent to the view so JS can lock those roles.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> OccupiedRoles { get; set; } = new();

        [Required]
        [Range(2, 100, ErrorMessage = "จำนวนผู้เล่นต้องอยู่ระหว่าง 2 - 100 คน")]
        public int MaxPlayers { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มกิจกรรม")]
        public DateTime StartEvent { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดกิจกรรม")]
        public DateTime EndEvent { get; set; }

        public DateTime StartRecruiting { get; set; }
        public DateTime EndRecruiting { get; set; }

        public void ApplyTo(Lobby existing)
        {
            existing.Title = this.Title;
            existing.Game = this.Game;
            existing.Description = this.Description;
            existing.Picture = this.Picture;
            existing.DiscordLink = this.DiscordLink;
            existing.Moods = this.Moods;
            existing.MaxPlayers = this.MaxPlayers;
            existing.StartRecruiting = this.StartRecruiting;
            existing.EndRecruiting = this.EndRecruiting;
            existing.StartEvent = this.StartEvent;
            existing.EndEvent = this.EndEvent;

            // ── Role protection ──────────────────────────────────────────────
            // 1. Collect role names currently held by active members (not pending)
            var takenRoles = existing.Members
                .Where(m => m.Status != "Pending" && !string.IsNullOrEmpty(m.Role))
                .Select(m => m.Role)
                .ToList();

            // 2. Start with the submitted roles (may include renamed empty slots + new roles)
            var merged = new List<string>(this.Roles);

            // 3. Any occupied role that is no longer in the submitted list must be re-added
            //    (host tried to delete a role that someone is holding — block it)
            foreach (var taken in takenRoles)
            {
                if (!merged.Contains(taken))
                    merged.Add(taken);
            }

            existing.Roles = merged;
        }
    }

}