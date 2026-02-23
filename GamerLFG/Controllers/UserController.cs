using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using MongoDB.Driver;
using GamerLFG.service;

namespace GamerLFG.Controllers;

public class UserController : Controller
{

    public IActionResult Friends_list()
    {
        return View();
    }

    public IActionResult Friends_request()
    {
        return View();
    }

    private readonly MongoDBservice _mongoDBservice;

    public UserController(MongoDBservice mongoDBservice)
    {
        _mongoDBservice = mongoDBservice;
    }

    [HttpGet]
    public async Task<IActionResult> Profiles(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _mongoDBservice.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string id, string Username, string Bio, string GameLibraryString, string VibeTagsString, string discord, string steam, string twitch)
    {
        var user = await _mongoDBservice.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound();

        // อัปเดตค่าใน Object
        user.Username = Username;
        user.Bio = Bio;
        user.discord = discord;
        user.steam = steam;
        user.twitch = twitch;

        if (!string.IsNullOrEmpty(GameLibraryString))
        {
            user.GameLibrary = GameLibraryString.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        if (!string.IsNullOrEmpty(VibeTagsString))
        {
            user.VibeTags = VibeTagsString.Split(',')
                .Select(v => v.Trim())
                .ToList();
        }
        else
        {
            user.VibeTags = new List<string>();
        }

        // --- ส่วนที่สำคัญที่สุด: บันทึกกลับลง DB ---
        await _mongoDBservice.Users.ReplaceOneAsync(u => u.Id == id, user);

        return RedirectToAction("Profiles", new { id = id });
    }
}
