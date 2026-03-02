using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GamerLFG.Models;
using System.Text.Json;
using System.Linq;
using MongoDB.Bson;
using System.Runtime.CompilerServices;
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
        public string Title { get; set; } //
        
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

        // Tags สำหรับเลือกแนวการเล่น เช่น "Chill", "Serious"
        public List<string> Moods { get; set;} = new(); //

        // ตำแหน่งที่ต้องการ เช่น "Tank", "Healer"
        public List<string> Roles { get; set; } = new(); //
        public string HostRole { get; set; } = "All Class";

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
        [Required(ErrorMessage = "กรุณาระบุเวลาเริ่มสมัครกิจกรรม")]
        public DateTime StartRecruiting { get; set; }
        [Required(ErrorMessage = "กรุณาระบุเวลาสิ้นสุดรับสมัครกิจกรรม")]
        public DateTime EndRecruiting { get; set; }

        public (bool valid, string erMessage) TimeValidation()
        {   
            DateTime minAllowedTime = DateTime.UtcNow.AddMinutes(10);
            if(this.StartRecruiting < this.EndRecruiting )
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
            Console.WriteLine(jsonContent);
            List<string> rawRoles = new List<string>();
            if (!string.IsNullOrEmpty(jsonContent) && jsonContent != "[]")
            {
                // 2. Parse ก้อน JSON string นั้น
                using JsonDocument doc = JsonDocument.Parse(jsonContent);

                // 3. วนลูปใน Array แล้วดึง Text ของแต่ละก้อนออกมาเป็น List<string>
               rawRoles = doc.RootElement.EnumerateArray()
                    .Select(item => item.GetRawText())
                    .ToList();


            }
            else
            {
                rawRoles = new List<string> { $"{{\"label\": \"All Class\", \"quantity\": {this.MaxPlayers}}}"};
            }
            // Console.WriteLine(this.Moods.GetType());
            return new Lobby
            {
                
                Title = this.Title, //
                Game = this.Game, //
                Description = this.Description, //
                HostId = this.HostId,
                HostName = this.HostName,
                Picture = this.Picture, //
                DiscordLink = this.DiscordLink, //
                Moods = this.Moods, //
                Roles = rawRoles, //
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

    
    // [BsonRepresentation(BsonType.ObjectId)]
    // public string HostId { get; set; }
    // public string HostName {get; set;}S

}