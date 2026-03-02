using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GamerLFG.Models;
using System.Text.Json;
using System.Linq;
using MongoDB.Bson;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
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
        public int CurrentPlayers {get;set;}
        public int MaxPlayers {get;set;}

        public bool isRecuiting {get;set;}
        public DateTime EndEvent {get;set;}
    }

    public class CreateLobbyDTO
    {
        [Required(ErrorMessage = "กรุณาระบุชื่อห้อง")]
        [StringLength(100, ErrorMessage = "ชื่อห้องยาวเกินไป")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "กรุณาระบุชื่อเกม")]
        public string Game { get; set; }

        [Required(ErrorMessage = "กรุณาใส่รายละเอียด (Description อะ)")]
        public string Description { get; set; }//
        
        public string HostId {get;set;}
        public string HostName {get;set;}
        public string Picture { get; set; } 
        = "https://pbs.twimg.com/profile_images/1871124858840752128/pLV1ZYMU_400x400.jpg"//
;
        [Required(ErrorMessage = "กรุณาใส่ลิงก์ Discord เพื่อใช้สื่อสาร")]
        [Url(ErrorMessage = "รูปแบบลิงก์ไม่ถูกต้อง")]
        public string DiscordLink { get; set; }

        public List<string> Moods { get; set; } = new();

        /// <summary>Role slots with quotas, bound from form via Roles[i].Name / Roles[i].Quantity.</summary>
        public List<string> Roles { get; set; } = new();
        public string HostRole { get; set; } = "All Class";

        [Required]
        [Range(2, 100, ErrorMessage = "จำนวนผู้เล่นต้องอยู่ระหว่าง 2 - 100 คน")]
        public int MaxPlayers { get; set; }
       
        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มกิจกรรม")]
        public DateTime StartEvent { get; set; }

        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดกิจกรรม")]
        public DateTime EndEvent { get; set; }

        // หมายเหตุ: Start/End Recruiting อาจจะตั้งค่า Default 
        // หรือรับมาจากหน้าฟอร์มก็ได้ แล้วแต่การออกแบบ UI ของคุณ
        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มสมัครกิจกรรม")]
        public DateTime StartRecruiting { get; set; }
        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดรับสมัครกิจกรรม")]
        public DateTime EndRecruiting { get; set; }

        public (bool valid, string erMessage) TimeValidation()
        {   
            DateTime minAllowedTime = DateTime.UtcNow.AddMinutes(10);
            if(this.StartRecruiting > this.EndRecruiting )
            {
                return (false, "เวลาเริ่มรับสมัครต้องก่อนเวลาปิดรับสมัคร");
            }
            if(this.StartRecruiting < minAllowedTime)
            {
                return (false,"เวลาเริ่ม Recuiting ต้องไม่เป็นอดีต");
            
            }
            if(this.StartEvent < this.EndRecruiting)
            {
                return (false,"เวลา StartEvent ต้องมากกว่าเวลา EndRecuituing");
            }
            if(this.StartEvent >= this.EndEvent )
            {
                return (false,"เวลา StartEvent ต้องไม่มากกว่าหรือเท่ากับเวลา EndEvent");
            }
            return (true,"Time is Valid");
        }
        public Lobby ToEntity()
        {
            // Console.WriteLine(this.Roles);


            string jsonContent = this.Roles?.FirstOrDefault();
            List<LobbyRole> lobbyRoles = new List<LobbyRole>();

            if (!string.IsNullOrEmpty(jsonContent) && jsonContent != "[]")
            {
                using JsonDocument doc = JsonDocument.Parse(jsonContent);
                
                lobbyRoles = doc.RootElement.EnumerateArray()
                    .Select(item => new LobbyRole {
                        Name = item.GetProperty("label").GetString(),
                        Quantity = item.GetProperty("quantity").GetInt32()
                    })
                    .ToList();
            }
            else
            {
                lobbyRoles.Append(new LobbyRole {
                        Name = "Others",
                        Quantity = this.MaxPlayers
                    });
            }
            // Console.WriteLine(this.Moods.GetType());
            return new Lobby
            {
                
                Title = this.Title, //
                Game = this.Game, //
                Description = this.Description, //
                HostId = this.HostId,
                // HostName = this.HostName,
                Picture = this.Picture, //
                DiscordLink = this.DiscordLink, //
                Moods = this.Moods, //
                Roles = lobbyRoles, //
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