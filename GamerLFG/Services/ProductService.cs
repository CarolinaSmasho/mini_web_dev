using MongoDB.Driver;
using GamerLFG.Models;
using GamerLFG.service;

namespace GamerLFG.Services;

public class ProductService
{
    private readonly MongoDBservice _database;

    public ProductService(MongoDBservice database)
    {
        _database = database;
    }

    // ดึงสินค้าทั้งหมดจาก MongoDB
    public async Task<List<Product>> GetAllAsync()
    {
        return await _database.Products.Find(_ => true).ToListAsync();
    }

    // Seed ข้อมูลตัวอย่าง (เรียกตอน startup ถ้า collection ว่าง)
    public async Task SeedAsync()
    {
        var count = await _database.Products.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var sampleProducts = new List<Product>
        {
            new() { Name = "iPhone 16", Price = 35900, Category = "มือถือ", Description = "iPhone รุ่นล่าสุด" },
            new() { Name = "MacBook Air M3", Price = 42900, Category = "โน้ตบุ๊ก", Description = "โน้ตบุ๊กสำหรับงานทั่วไป" },
            new() { Name = "AirPods Pro", Price = 9900, Category = "หูฟัง", Description = "หูฟัง Active Noise Cancellation" },
            new() { Name = "iPad Air", Price = 21900, Category = "แท็บเล็ต", Description = "แท็บเล็ตสำหรับสร้างสรรค์" },
            new() { Name = "Apple Watch S10", Price = 15900, Category = "สมาร์ทวอทช์", Description = "นาฬิกาสุขภาพ" },
        };

        await _database.Products.InsertManyAsync(sampleProducts);
    }

}
