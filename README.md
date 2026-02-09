# 🎮 GamerLFG

GamerLFG เป็นแพลตฟอร์มสำหรับเกมเมอร์ในการหาเพื่อนร่วมเล่นเกม (Looking For Group) พัฒนาด้วย ASP.NET Core MVC และ MongoDB

---

## 📋 สารบัญ (Table of Contents)
- [สิ่งที่ต้องมี (Prerequisites)](#-สิ่งที่ต้องมี-prerequisites)
- [การตั้งค่า (Setup)](#-การตั้งค่า-setup)
- [วิธีการรันโปรเจค (Running)](#-วิธีการรันโปรเจค-running)
- [โครงสร้างโปรเจค (Project Structure)](#-โครงสร้างโปรเจค-project-structure)
- [การตั้งค่าเพิ่มเติม (Configuration)](#-การตั้งค่าเพิ่มเติม-configuration)

---

## ⚙️ สิ่งที่ต้องมี (Prerequisites)

| เครื่องมือ | เวอร์ชัน | ลิงก์ดาวน์โหลด |
|-----------|---------|--------------|
| .NET SDK | 10.0 หรือล่าสุด | [ดาวน์โหลด](https://dotnet.microsoft.com/download) |
| MongoDB | 6.0+ | [ดาวน์โหลด](https://www.mongodb.com/try/download/community) |
| Git | (ล่าสุด) | [ดาวน์โหลด](https://git-scm.com/) |

---

## 🚀 การตั้งค่า (Setup)

### 1. คลอนโปรเจค (Clone Repository)
```bash
git clone <repository-url>
cd mini_webdev
```

### 2. เข้าไปในโฟลเดอร์โปรเจค
```bash
cd GamerLFG
```

### 3. คืนค่า Dependencies
```bash
dotnet restore
```

### 4. ตั้งค่า MongoDB
ตรวจสอบว่า MongoDB รันอยู่บน `localhost:27017` หรือแก้ไขการตั้งค่าใน `appsettings.json`:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "GamerLFG"
  }
}
```

> 💡 **หมายเหตุ:** หากใช้ MongoDB Atlas ให้เปลี่ยน `ConnectionString` เป็น Connection String ของ Atlas แทน

---

## ▶️ วิธีการรันโปรเจค (Running)

### รันโปรเจค (Development Mode)
```bash
cd GamerLFG
dotnet run
```
เข้าใช้งานได้ที่: `http://localhost:5000` หรือ `https://localhost:5014`

### รันพร้อม Hot Reload (แนะนำสำหรับ Development)
```bash
cd GamerLFG
dotnet watch run
```
> 🔥 โค้ดจะ Reload อัตโนมัติเมื่อมีการแก้ไขไฟล์

### สร้างไฟล์สำหรับ Production
```bash
cd GamerLFG
dotnet build -c Release
```

### รัน Production Build
```bash
cd GamerLFG
dotnet run -c Release
```

---

## 📁 โครงสร้างโปรเจค (Project Structure)

```
mini_webdev/
├── GamerLFG/                    # โปรเจคหลัก
│   ├── Controllers/             # MVC Controllers
│   ├── Models/                  # Data Models
│   ├── Views/                   # Razor Views
│   ├── Repositories/            # Data Access Layer
│   ├── Services/                # Business Logic
│   ├── Data/                    # Database Context
│   ├── wwwroot/                 # Static Files (CSS, JS, Images)
│   ├── Program.cs               # Entry Point
│   ├── appsettings.json         # Configuration
│   └── GamerLFG.csproj          # Project File
├── README.md                    # ไฟล์นี้
├── api-contract.md              # API Documentation
├── features.md                  # Features Specification
└── diagram.md                   # Architecture Diagrams
```

---

## 🔧 การตั้งค่าเพิ่มเติม (Configuration)

### 🔗 MongoDB Connection
แก้ไขไฟล์ `GamerLFG/appsettings.json`:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "GamerLFG"
  }
}
```

### 🌐 Environment-Specific Settings
- **Development:** `appsettings.Development.json`
- **Production:** `appsettings.Production.json`

---

## 📖 API Documentation

หลังจากรันโปรเจคแล้ว สามารถเข้าดู Swagger API Documentation ได้ที่:
```
https://localhost:5001/swagger
```

---

## 🛠️ Commands ที่ใช้บ่อย (Useful Commands)

| คำสั่ง | คำอธิบาย |
|-------|---------|
| `dotnet restore` | คืนค่า NuGet packages |
| `dotnet build` | Build โปรเจค |
| `dotnet run` | รันโปรเจค |
| `dotnet watch run` | รันพร้อม Hot Reload |
| `dotnet clean` | ลบไฟล์ build |
| `dotnet build -c Release` | Build สำหรับ Production |

---

