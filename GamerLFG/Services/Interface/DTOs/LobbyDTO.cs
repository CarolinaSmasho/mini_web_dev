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
        public string Title { get; set; }
        
        [Required(ErrorMessage = "กรุณาระบุชื่อเกม")]
        public string Game { get; set; }

        public string Description { get; set; }
        public string HostId {get;set;}
        public string Picture { get; set; }

        [Required(ErrorMessage = "กรุณาใส่ลิงก์ Discord เพื่อใช้สื่อสาร")]
        [Url(ErrorMessage = "รูปแบบลิงก์ไม่ถูกต้อง")]
        public string DiscordLink { get; set; }

        public List<string> Moods { get; set; } = new();

        /// <summary>Role slots with quotas, bound from form via Roles[i].Name / Roles[i].Quantity.</summary>
        public List<LobbyRole> Roles { get; set; } = new();
        public string HostRole { get; set; } = "GAY";

        [Required]
        [Range(2, 100, ErrorMessage = "จำนวนผู้เล่นต้องอยู่ระหว่าง 2 - 100 คน")]
        public int MaxPlayers { get; set; }
       
        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มกิจกรรม")]
        public DateTime StartEvent { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดกิจกรรม")]
        public DateTime EndEvent { get; set; }

        public DateTime StartRecruiting { get; set; }
        public DateTime EndRecruiting { get; set; }

        public Lobby ToEntity()
        {
            return new Lobby
            {
                Title = this.Title,
                Game = this.Game,
                Description = this.Description,
                Picture = this.Picture,
                DiscordLink = this.DiscordLink,
                Moods = this.Moods,
                Roles = this.Roles,
                MaxPlayers = this.MaxPlayers,
                StartRecruiting = this.StartRecruiting,
                EndRecruiting = this.EndRecruiting,
                StartEvent = this.StartEvent,
                EndEvent = this.EndEvent,
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
    }

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

        /// <summary>Role slots with quotas, bound from form via Roles[i].Name / Roles[i].Quantity.</summary>
        public List<LobbyRole> Roles { get; set; } = new();

        /// <summary>
        /// Role names currently held by active members.
        /// NOT bound from form — server-side only, passed to View for JS locking.
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
            // Collect role names currently held by active members
            var takenRoleNames = existing.Members
                .Where(m => m.Status != "Pending" && !string.IsNullOrEmpty(m.Role))
                .Select(m => m.Role)
                .Distinct()
                .ToList();

            // Start from submitted roles
            var merged = new List<LobbyRole>(this.Roles);

            // Re-add any occupied role that the host tried to delete (protection)
            foreach (var takenName in takenRoleNames)
            {
                if (!merged.Any(r => r.Name == takenName))
                {
                    // Restore with quantity 1 as minimum fallback
                    merged.Add(new LobbyRole { Name = takenName, Quantity = 1 });
                }
            }

            existing.Roles = merged;
        }
    }

}