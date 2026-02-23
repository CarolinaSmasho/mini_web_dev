using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.service;
using MongoDB.Driver;

namespace GamerLFG.Controllers
{
    public class KarmaController : Controller
    {
        private readonly MongoDBservice _mongoDBservice;

        public KarmaController(MongoDBservice mongoDBservice)
        {
            _mongoDBservice = mongoDBservice;
        }

        [HttpGet]
        public async Task<IActionResult> KarmaHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await _mongoDBservice.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            var KarmaHistory = await _mongoDBservice.KarmaHistories
            .Find(k => k.TargetUserId == userId)
            .SortByDescending(k => k.Date)
            .ToListAsync();

            ViewBag.Username = user.Username;
            ViewBag.TotalScore = KarmaHistory.Sum(k => k.Score);

            return View(KarmaHistory);
        }
    }
}