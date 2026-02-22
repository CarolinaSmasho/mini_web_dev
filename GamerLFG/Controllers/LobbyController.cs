using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using System.Collections.Generic;
using GamerLFG.Services;
using GamerLFG.Services.Interface.DTOs;
namespace GamerLFG.Controllers
{
    public class LobbyController : Controller
    {
        // 1. หน้าแสดงรายละเอียด (ที่ใช้ MockData ใน View)
        // เข้าผ่าน: http://localhost:PORT/Lobby/Details/1
        private readonly LobbyService _lobbyService;
        public LobbyController(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }
        public IActionResult Details(string id)
        {
            // ตอนนี้เราส่ง Model เปล่าไปก่อน เพราะคุณเขียน MockData ไว้ในหน้า View แล้ว
            // แต่ต้องส่งไปเพื่อให้ @model GamerLFG.Models.Lobby ไม่ Error
            var lobby = new Lobby(); 
            return View(lobby);
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- ส่วนของ API ที่เรียกใช้จาก JavaScript (fetch) ในหน้า View ---

        [HttpPost]
        public IActionResult Apply(string id)
        {
            // จำลองการกดสมัคร
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Recruit(string appId)
        {
            // จำลองการรับคนเข้าทีม
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult ToggleRecruitment(string id)
        {
            // จำลองการเปิด-ปิดห้อง
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create_lobby(CreateLobbyDTO model)
        {
            if (ModelState.IsValid)
            {
                await _lobbyService.CreateLobbyAsync(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }
       
    }
}