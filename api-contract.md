# 📋 API Contract - Gamer LFG

เอกสารนี้เป็น **API Contract** สำหรับแอป Gamer LFG (Looking For Group)  
ใช้เป็นข้อตกลงระหว่าง **Frontend (HTML/CSS/JS)** และ **Backend (ASP.Net MVC)**

---

## 📌 สรุป Entities หลัก

| Entity | คำอธิบาย |
|--------|---------|
| **User** | ผู้ใช้งาน (มี Karma Score, Vibe Tags, Game Library) |
| **Lobby** | ห้องหาเพื่อน (มีเกม, จำนวนคน, Role, Mood, Schedule) |
| **Application** | คำขอเข้าร่วม Lobby |
| **Message** | ข้อความใน Lobby Chat |
| **Endorsement** | การให้คะแนน/รีวิวผู้เล่น |

---

## 🔐 Authentication APIs

### POST `/api/auth/register`
**ลงทะเบียนผู้ใช้ใหม่**

**Request Body:**
```json
{
  "username": "Notatord",
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "userId": 1,
    "username": "Notatord",
    "email": "user@example.com"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": "Username already exists"
}
```

---

### POST `/api/auth/login`
**เข้าสู่ระบบ**

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securepassword123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "username": "Notatord",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

---

### POST `/api/auth/logout`
**ออกจากระบบ**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

## 👤 User APIs

### GET `/api/users/{id}`
**ดูโปรไฟล์ผู้ใช้**

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "userId": 1,
    "username": "Notatord",
    "avatar": "https://example.com/avatar.jpg",
    "karmaScore": 4.8,
    "vibeTags": ["Tryhard", "Mic ON", "Team Player"],
    "gameLibrary": ["Elden Ring", "Valorant", "Apex Legends"],
    "endorsements": {
      "strategist": 15,
      "friendly": 23,
      "skilled": 18
    },
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

---

### PUT `/api/users/{id}`
**แก้ไขโปรไฟล์**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "avatar": "https://example.com/new-avatar.jpg",
  "vibeTags": ["Chill", "No Mic", "Casual"],
  "gameLibrary": ["Elden Ring", "Valorant", "Minecraft"]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Profile updated successfully"
}
```

---

### POST `/api/users/{id}/endorse`
**ให้ Endorsement แก่ผู้เล่น**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "endorsementType": "strategist",
  "comment": "Great leader in our raid!"
}
```

**Endorsement Types:**
- `strategist` - สั่งการดี
- `friendly` - สุภาพ, ดี
- `skilled` - เก่ง, แบกทีม
- `teacher` - สอนเล่นเก่ง
- `reliable` - ไว้ใจได้, ไม่เท

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Endorsement sent!"
}
```

---

## 🎮 Lobby APIs

### GET `/api/lobbies`
**ดึงรายการ Lobby ทั้งหมด (Public Feed)** ⚡ **ใช้ Ajax**

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `game` | string | กรองตามเกม (optional) |
| `mood` | string | กรองตาม mood (optional) |
| `status` | string | `open`, `full`, `closed` (optional) |
| `page` | int | หน้าที่ต้องการ (default: 1) |
| `limit` | int | จำนวนต่อหน้า (default: 10) |

**Example Request:**
```
GET /api/lobbies?game=Valorant&status=open&page=1&limit=10
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "lobbies": [
      {
        "lobbyId": 1,
        "title": "Elden Ring Boss Hunt - Malenia",
        "game": "Elden Ring",
        "description": "Need a tank and a healer. Mic required.",
        "currentPlayers": 2,
        "maxPlayers": 3,
        "host": {
          "userId": 1,
          "username": "Notatord",
          "avatar": "https://example.com/avatar.jpg",
          "karmaScore": 4.8
        },
        "moods": ["Tryhard", "Mic On", "Boss Rush"],
        "roles": [
          { "name": "Tank/Aggro", "count": 1 }
        ],
        "status": "open",
        "isRecruiting": true,
        "scheduledTime": "2026-02-01T20:00:00Z",
        "createdAt": "2026-01-31T17:00:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "totalPages": 5,
      "totalCount": 45
    }
  }
}
```

---

### GET `/api/lobbies/{id}`
**ดูรายละเอียด Lobby**

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "lobbyId": 1,
    "title": "Elden Ring Boss Hunt - Malenia",
    "game": "Elden Ring",
    "description": "Need a tank and a healer. Mic required.",
    "currentPlayers": 2,
    "maxPlayers": 3,
    "host": {
      "userId": 1,
      "username": "Notatord",
      "avatar": "https://example.com/avatar.jpg",
      "karmaScore": 4.8
    },
    "moods": ["Tryhard", "Mic On", "Boss Rush"],
    "roles": [
      { "name": "Tank/Aggro", "count": 1, "filled": 0 }
    ],
    "duration": "Until loss",
    "status": "open",
    "isRecruiting": true,
    "scheduledTime": "2026-02-01T20:00:00Z",
    "createdAt": "2026-01-31T17:00:00Z",
    "members": [
      {
        "userId": 1,
        "username": "Notatord",
        "avatar": "https://example.com/avatar.jpg",
        "role": "Host/Leader",
        "isHost": true
      }
    ]
  }
}
```

---

### POST `/api/lobbies`
**สร้าง Lobby ใหม่** ⚡ **ใช้ Ajax**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Ranked Grind to Diamond",
  "game": "Valorant",
  "description": "Must have good comms. No toxicity.",
  "maxPlayers": 5,
  "moods": ["Serious", "Competitive", "Mic On"],
  "roles": [
    { "name": "Duelist", "count": 1 },
    { "name": "Smokes", "count": 1 },
    { "name": "Sentinel", "count": 1 }
  ],
  "duration": "3 hrs",
  "scheduledTime": "2026-02-01T20:00:00Z"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Lobby created successfully",
  "data": {
    "lobbyId": 123,
    "title": "Ranked Grind to Diamond",
    "status": "open",
    "createdAt": "2026-01-31T17:44:00Z"
  }
}
```

---

### PUT `/api/lobbies/{id}`
**แก้ไข Lobby**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Ranked Grind to Immortal",
  "description": "Updated description",
  "maxPlayers": 6,
  "moods": ["Tryhard", "Competitive"],
  "roles": [
    { "name": "Duelist", "count": 2 },
    { "name": "Controller", "count": 1 }
  ],
  "isRecruiting": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lobby updated successfully"
}
```

---

### DELETE `/api/lobbies/{id}`
**ลบ Lobby**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lobby deleted successfully"
}
```

---

### PUT `/api/lobbies/{id}/status`
**เปลี่ยนสถานะ Lobby (ปิด/เปิดรับสมัคร)**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "isRecruiting": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lobby status updated"
}
```

---

## 📝 Application APIs

### POST `/api/lobbies/{id}/apply`
**ขอเข้าร่วม Lobby** ⚡ **ใช้ Ajax**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "desiredRoles": ["Smokes", "Controller"],
  "message": "I'm experienced with smokes. Looking to grind!"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Application submitted!",
  "data": {
    "applicationId": 456,
    "status": "pending"
  }
}
```

---

### DELETE `/api/lobbies/{id}/apply`
**ยกเลิกคำขอเข้าร่วม**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Application cancelled"
}
```

---

### GET `/api/lobbies/{id}/applicants`
**ดูรายชื่อผู้สมัครเข้า Lobby (Host Only)**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "applicants": [
      {
        "applicationId": 456,
        "user": {
          "userId": 2,
          "username": "WarriorX",
          "avatar": "https://example.com/avatar2.jpg",
          "karmaScore": 4.8,
          "vibeTags": ["Tryhard", "Mic ON"],
          "gameLibrary": ["Elden Ring", "Dark Souls 3", "Sekiro"]
        },
        "desiredRoles": ["Tank/Aggro"],
        "message": "Ready to tank!",
        "votes": {
          "yes": 2,
          "no": 0
        },
        "appliedAt": "2026-01-31T18:00:00Z"
      }
    ]
  }
}
```

---

### POST `/api/lobbies/{id}/applicants/{applicationId}/recruit`
**รับผู้สมัครเข้าทีม (Recruit)**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "assignedRole": "Tank/Aggro"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "WarriorX has been recruited as Tank/Aggro!"
}
```

---

### POST `/api/lobbies/{id}/applicants/{applicationId}/reject`
**ปฏิเสธผู้สมัคร (Pass)**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Application rejected"
}
```

---

### POST `/api/lobbies/{id}/applicants/{applicationId}/vote`
**โหวตรับ/ไม่รับผู้สมัคร (Squad Voting)**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "vote": "yes"
}
```

**Vote Options:** `yes` | `no`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Vote recorded",
  "data": {
    "votes": {
      "yes": 3,
      "no": 0
    }
  }
}
```

---

## 💬 Chat APIs

### GET `/api/lobbies/{id}/messages`
**ดึงข้อความใน Lobby Chat** ⚡ **ใช้ Ajax (Polling)**

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `after` | datetime | ดึงข้อความหลังเวลานี้ (optional) |
| `limit` | int | จำนวนข้อความ (default: 50) |

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "messages": [
      {
        "messageId": 1,
        "sender": {
          "userId": 1,
          "username": "Notatord",
          "avatar": "https://example.com/avatar.jpg",
          "isHost": true
        },
        "content": "ยินดีต้อนรับทุกคนครับ!",
        "sentAt": "2026-01-31T18:00:00Z"
      },
      {
        "messageId": 2,
        "sender": {
          "userId": 2,
          "username": "WarriorX",
          "avatar": "https://example.com/avatar2.jpg",
          "isHost": false
        },
        "content": "พร้อมลุยครับ!",
        "sentAt": "2026-01-31T18:01:00Z"
      }
    ]
  }
}
```

---

### POST `/api/lobbies/{id}/messages`
**ส่งข้อความใน Lobby Chat** ⚡ **ใช้ Ajax**

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "content": "เดี๋ยวรอ 5 นาทีนะครับ"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "messageId": 3,
    "content": "เดี๋ยวรอ 5 นาทีนะครับ",
    "sentAt": "2026-01-31T18:05:00Z"
  }
}
```

---

## 📊 My Lobbies API

### GET `/api/users/me/lobbies`
**ดึง Lobby ที่ตัวเองเกี่ยวข้อง**

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "myLobbies": [
      {
        "lobbyId": 1,
        "title": "Elden Ring Boss Hunt",
        "game": "Elden Ring",
        "status": "open",
        "currentPlayers": 2,
        "maxPlayers": 3,
        "role": "host"
      }
    ],
    "pendingRequests": [
      {
        "lobbyId": 2,
        "title": "Ranked Grind",
        "game": "Valorant",
        "applicationStatus": "pending",
        "appliedAt": "2026-01-31T17:30:00Z"
      }
    ],
    "joinedLobbies": [
      {
        "lobbyId": 3,
        "title": "Chill Apex Games",
        "game": "Apex Legends",
        "assignedRole": "Member"
      }
    ]
  }
}
```

---

## 🔔 HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200 OK` | สำเร็จ |
| `201 Created` | สร้างข้อมูลใหม่สำเร็จ |
| `400 Bad Request` | ข้อมูลไม่ถูกต้อง |
| `401 Unauthorized` | ไม่ได้ Login หรือ Token หมดอายุ |
| `403 Forbidden` | ไม่มีสิทธิ์เข้าถึง |
| `404 Not Found` | ไม่พบข้อมูล |
| `409 Conflict` | ข้อมูลซ้ำ (เช่น สมัครซ้ำ) |
| `500 Internal Server Error` | Server มีปัญหา |

---

## 🔧 Standard Error Response Format

```json
{
  "success": false,
  "error": "Error message here",
  "errorCode": "LOBBY_FULL",
  "details": {
    "field": "maxPlayers",
    "message": "Lobby has reached maximum capacity"
  }
}
```

---

## 📝 Notes for Implementation

### Frontend (HTML/CSS/JS + Ajax)
1. ใช้ `fetch()` หรือ `XMLHttpRequest` สำหรับ Ajax calls
2. เก็บ Token ใน `localStorage` หรือ `sessionStorage`
3. แนบ Token ใน Header ทุก Request ที่ต้องการ Authentication

### Backend (ASP.Net MVC)
1. สร้าง Controllers ตาม Resources: `AuthController`, `UserController`, `LobbyController`, `MessageController`
2. ใช้ Entity Framework สำหรับ Database
3. Implement JWT หรือ ASP.Net Identity สำหรับ Authentication

---

## 🚀 Ajax Endpoints Summary (ตาม Requirement)

| Endpoint | Purpose | ใช้ Ajax |
|----------|---------|---------|
| `GET /api/lobbies` | โหลด Lobby Feed | ✅ |
| `POST /api/lobbies` | สร้าง Lobby | ✅ |
| `POST /api/lobbies/{id}/apply` | ขอเข้าร่วม | ✅ |
| `GET /api/lobbies/{id}/messages` | Polling ข้อความ | ✅ |
| `POST /api/lobbies/{id}/messages` | ส่งข้อความ | ✅ |

> ⚠️ **หมายเหตุ**: ตาม requirement ต้องใช้ Ajax อย่างน้อย 1 ที่ - เอกสารนี้แนะนำ 5 endpoints ที่เหมาะกับ Ajax

---

*Document Version: 1.0*  
*Last Updated: 2026-01-31*
