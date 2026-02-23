using MongoDB.Driver;
using GamerLFG.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Data
{
    public static class MongoDbSeeder
    {
        public static async Task SeedAsync(IMongoDatabase database)
        {
            var userCollection = database.GetCollection<User>("Users");

            // 1. เตรียมลิสต์ Mockup Users ของคุณ (เพิ่มคนใหม่ต่อท้ายได้เลย)
            var mockUsers = new List<User>
            {
                new User
                    {
                        Id = "65d8a0b1c2d3e4f5a6b7c8d1",
                        Username = "ProSniper99",
                        Email = "sniper@gamerlfg.com",
                        Password = "hashed_password_mock_1",
                        Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=ProSniper99",
                        Bio = "เน้นเล่นเกมแนว FPS ชอบลงแรงค์จริงจัง มองหาตี้ที่สื่อสารตลอดเวลา",
                        KarmaScore = 95.5,
                        VibeTags = new List<string> { "Competitive", "Mic Required", "Tryhard" },
                        GameLibrary = new List<string> { "Valorant", "CS2", "Apex Legends" },
                        FriendIds = new List<string> { "65d8a0b1c2d3e4f5a6b7c8d2", "65d8a0b1c2d3e4f5a6b7c8d3" },
                        SocialMedia = new List<string> { "discord.gg/prosniper" },
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = "65d8a0b1c2d3e4f5a6b7c8d2",
                        Username = "CozyGamerGirl",
                        Email = "cozy@gamerlfg.com",
                        Password = "hashed_password_mock_2",
                        Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=CozyGamer",
                        Bio = "เล่นเกมชิลๆ ปลูกผัก สร้างบ้าน เน้นคุยสนุกไม่หัวร้อนค่ะ",
                        KarmaScore = 120.0,
                        VibeTags = new List<string> { "Chill", "Casual", "Beginner Friendly" },
                        GameLibrary = new List<string> { "Stardew Valley", "Minecraft", "Palworld" },
                        FriendIds = new List<string> { "65d8a0b1c2d3e4f5a6b7c8d1" },
                        SocialMedia = new List<string> { "twitter.com/cozygamer" },
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = "65d8a0b1c2d3e4f5a6b7c8d3",
                        Username = "RaidLeaderBob",
                        Email = "bob.tank@gamerlfg.com",
                        Password = "hashed_password_mock_3",
                        Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Bob",
                        Bio = "หาคนลงดันเจี้ยนวันหยุดสุดสัปดาห์ ขาด Healer 1 ตำแหน่งครับ",
                        KarmaScore = 88.0,
                        VibeTags = new List<string> { "Roleplay", "Team Player", "Scheduled" },
                        GameLibrary = new List<string> { "Final Fantasy XIV", "World of Warcraft", "Diablo IV" },
                        FriendIds = new List<string> { "65d8a0b1c2d3e4f5a6b7c8d1" },
                        SocialMedia = new List<string> { "twitch.tv/raidbob" },
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = "65d8a0b1c2d3e4f5a6b7c8d4",
                        Username = "aaaaaaaa",
                        Email = "bob.tank@gamerlfg.com",
                        Password = "hashed_password_mock_4",
                        Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Bob",
                        Bio = "หาคนลงดันเจี้ยนวันหยุดสุดสัปดาห์ ขาด Healer 5 ตำแหน่งครับ",
                        KarmaScore = 88.0,
                        VibeTags = new List<string> { "Roleplay", "Team Player", "Scheduled" },
                        GameLibrary = new List<string> { "Final Fantasy XIV", "World of Warcraft", "Diablo IV" },
                        FriendIds = new List<string> { "65d8a0b1c2d3e4f5a6b7c8d1" },
                        SocialMedia = new List<string> { "twitch.tv/raidbob" },
                        CreatedAt = DateTime.UtcNow
                    },
                
                // 🟢 เพิ่ม User คนใหม่ตรงนี้ได้เลย! กำหนด ObjectId ให้ต่างจากเดิมเล็กน้อย
                new User 
                { 
                    Id = "65d8a0b1c2d3e4f5a6b7c8d5", 
                    Username = "NewGamerTest", 
                    Email = "newbie@gamerlfg.com",
                    Password = "hashed_password_mock_4",
                    Bio = "เพิ่งสมัครใหม่ครับ ฝากเนื้อฝากตัวด้วย",
                    CreatedAt = DateTime.UtcNow
                }
            };

            // 2. วนลูปเช็คทีละคน และทำการ Upsert
            foreach (var user in mockUsers)
            {
                // เงื่อนไข: ค้นหาด้วย Id
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                
                // ตั้งค่า IsUpsert = true (ถ้าเจออัปเดต ถ้าไม่เจอเพิ่มใหม่)
                var options = new ReplaceOptions { IsUpsert = true };

                // สั่งทำงาน! (เอา Object user ตัวใหม่ ไปทับตัวเก่า หรือสร้างใหม่)
                await userCollection.ReplaceOneAsync(filter, user, options);
            }

            Console.WriteLine("✅ Database Seeding: Successfully Synced / Upserted mock users.");
        }
    }
}