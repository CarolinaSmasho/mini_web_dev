# 📊 System Diagrams - Gamer LFG

เอกสารนี้แสดง Class Diagram และ Entity Relationship เพื่ออธิบายโครงสร้างของระบบ Gamer LFG

---
ctrl+shift+v เพื่อดู
## 🏗️ Class Diagram

```mermaid
classDiagram
    class User {
        +int userId
        +string username
        +string email
        +string password
        +string avatar
        +float karmaScore
        +string[] vibeTags
        +string[] gameLibrary
        +datetime createdAt
        +register()
        +login()
        +logout()
        +updateProfile()
        +endorse(User)
    }

    class Lobby {
        +int lobbyId
        +string title
        +string game
        +string description
        +int currentPlayers
        +int maxPlayers
        +string[] moods
        +Role[] roles
        +string status
        +bool isRecruiting
        +datetime scheduledTime
        +datetime createdAt
        +create()
        +update()
        +delete()
        +openRecruitment()
        +closeRecruitment()
        +getMissingRoles()
    }

    class Role {
        +string name
        +int count
        +int filled
    }

    class Application {
        +int applicationId
        +string[] desiredRoles
        +string message
        +string status
        +int yesVotes
        +int noVotes
        +datetime appliedAt
        +submit()
        +cancel()
        +recruit(string role)
        +reject()
    }

    class Vote {
        +int voteId
        +string voteType
        +datetime votedAt
        +castVote()
    }

    class Message {
        +int messageId
        +string content
        +datetime sentAt
        +send()
    }

    class Endorsement {
        +int endorsementId
        +string endorsementType
        +string comment
        +datetime createdAt
        +give()
    }

    class Member {
        +int memberId
        +string assignedRole
        +bool isHost
        +datetime joinedAt
    }

    %% Relationships
    User "1" --> "*" Lobby : hosts
    User "1" --> "*" Application : submits
    User "1" --> "*" Message : sends
    User "1" --> "*" Endorsement : gives
    User "1" --> "*" Endorsement : receives
    User "1" --> "*" Vote : casts
    
    Lobby "1" --> "*" Application : has
    Lobby "1" --> "*" Message : contains
    Lobby "1" --> "*" Role : defines
    Lobby "1" --> "*" Member : has
    
    Member "*" --> "1" User : is
    Application "*" --> "*" Vote : receives
```

---

## 🔄 Entity Relationship Diagram (ERD)

```mermaid
erDiagram
    USER {
        int userId PK
        string username UK
        string email UK
        string password
        string avatar
        float karmaScore
        datetime createdAt
    }
    
    USER_VIBE_TAG {
        int userId FK
        string tagName
    }
    
    USER_GAME {
        int userId FK
        string gameName
    }
    
    LOBBY {
        int lobbyId PK
        int hostId FK
        string title
        string game
        string description
        int maxPlayers
        string status
        bool isRecruiting
        datetime scheduledTime
        datetime createdAt
    }
    
    LOBBY_MOOD {
        int lobbyId FK
        string moodName
    }
    
    ROLE {
        int roleId PK
        int lobbyId FK
        string name
        int count
        int filled
    }
    
    MEMBER {
        int memberId PK
        int lobbyId FK
        int userId FK
        string assignedRole
        bool isHost
        datetime joinedAt
    }
    
    APPLICATION {
        int applicationId PK
        int lobbyId FK
        int userId FK
        string message
        string status
        datetime appliedAt
    }
    
    APPLICATION_ROLE {
        int applicationId FK
        string desiredRole
    }
    
    VOTE {
        int voteId PK
        int applicationId FK
        int voterId FK
        string voteType
        datetime votedAt
    }
    
    MESSAGE {
        int messageId PK
        int lobbyId FK
        int senderId FK
        string content
        datetime sentAt
    }
    
    ENDORSEMENT {
        int endorsementId PK
        int fromUserId FK
        int toUserId FK
        string endorsementType
        string comment
        datetime createdAt
    }
    
    KARMA_HISTORY {
        string id PK
        string userId FK
        string actionType
        int points
        string referenceType
        string referenceId
        string description
        datetime createdAt
    }
    
    USER ||--o{ USER_VIBE_TAG : has
    USER ||--o{ USER_GAME : owns
    USER ||--o{ LOBBY : hosts
    USER ||--o{ MEMBER : "is member"
    USER ||--o{ APPLICATION : submits
    USER ||--o{ VOTE : casts
    USER ||--o{ MESSAGE : sends
    USER ||--o{ ENDORSEMENT : gives
    USER ||--o{ ENDORSEMENT : receives
    USER ||--o{ KARMA_HISTORY : has
    
    LOBBY ||--o{ LOBBY_MOOD : has
    LOBBY ||--o{ ROLE : defines
    LOBBY ||--o{ MEMBER : contains
    LOBBY ||--o{ APPLICATION : receives
    LOBBY ||--o{ MESSAGE : contains
    
    APPLICATION ||--o{ APPLICATION_ROLE : requests
    APPLICATION ||--o{ VOTE : has
```

---

## 🔁 State Diagram - Lobby Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created : Host creates lobby
    Created --> Open : Start recruiting
    Open --> Full : Max players reached
    Open --> Closed : Host closes recruitment
    Full --> Open : Member leaves
    Closed --> Open : Host reopens
    Open --> Live : Scheduled time reached
    Full --> Live : Scheduled time reached
    Live --> Completed : Session ends
    Completed --> [*]
    
    note right of Open : isRecruiting = true
    note right of Closed : isRecruiting = false
    note right of Live : LIVE NOW badge shown
```

---

## 🔁 State Diagram - Application Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending : User applies
    Pending --> Accepted : Host recruits
    Pending --> Rejected : Host rejects
    Pending --> Cancelled : User cancels
    Accepted --> [*] : User becomes member
    Rejected --> [*]
    Cancelled --> [*]
    
    note right of Pending : Squad can vote
```

---

## 🧩 Object Diagram - Example Scenario

แสดงตัวอย่าง Instance ของ Objects ในระบบ:

```mermaid
classDiagram
    class Notatord {
        userId = 1
        username = "Notatord"
        karmaScore = 4.8
        vibeTags = ["Tryhard", "Mic ON"]
        gameLibrary = ["Elden Ring", "Valorant"]
    }

    class WarriorX {
        userId = 2
        username = "WarriorX"
        karmaScore = 4.5
        vibeTags = ["Team Player", "Chill"]
        gameLibrary = ["Elden Ring", "Dark Souls 3"]
    }

    class EldenRingLobby {
        lobbyId = 1
        title = "Elden Ring Boss Hunt - Malenia"
        game = "Elden Ring"
        maxPlayers = 3
        currentPlayers = 2
        status = "open"
        isRecruiting = true
        scheduledTime = "2026-02-01T20:00:00Z"
    }

    class TankRole {
        name = "Tank/Aggro"
        count = 1
        filled = 0
    }

    class WarriorXApplication {
        applicationId = 456
        desiredRoles = ["Tank/Aggro"]
        message = "Ready to tank!"
        status = "pending"
        yesVotes = 2
        noVotes = 0
    }

    class NotaMember {
        assignedRole = "Host/Leader"
        isHost = true
    }

    Notatord --> EldenRingLobby : hosts
    Notatord --> NotaMember : as member
    NotaMember --> EldenRingLobby : in lobby
    EldenRingLobby --> TankRole : needs
    WarriorX --> WarriorXApplication : submitted
    WarriorXApplication --> EldenRingLobby : applies to
```

---

## 📡 Sequence Diagram - Apply to Lobby Flow

```mermaid
sequenceDiagram
    participant U as User (WarriorX)
    participant F as Frontend
    participant B as Backend API
    participant DB as Database
    participant H as Host (Notatord)
    
    U->>F: View Lobby Details
    F->>B: GET /api/lobbies/{id}
    B->>DB: Query Lobby
    DB-->>B: Lobby Data
    B-->>F: Lobby Details + Members
    F-->>U: Display Pre-Join View
    
    U->>F: Select Role & Submit Application
    F->>B: POST /api/lobbies/{id}/apply
    B->>DB: Create Application
    DB-->>B: Application Created
    B-->>F: Success Response
    F-->>U: Show "Pending" Status
    
    Note over H: Host checks applicants
    H->>F: Open Manage Lobby > Applicants
    F->>B: GET /api/lobbies/{id}/applicants
    B->>DB: Query Applications
    DB-->>B: Applicants List
    B-->>F: Applicants with Votes
    F-->>H: Display Applicant Cards
    
    Note over H: Squad votes on applicant
    H->>F: Recruit WarriorX as Tank
    F->>B: POST /api/lobbies/{id}/applicants/{appId}/recruit
    B->>DB: Update Application + Add Member
    DB-->>B: Success
    B-->>F: "WarriorX recruited as Tank!"
    F-->>H: Update UI
    F-->>U: Notification: "You've been accepted!"
```

---

## 🎯 Use Case Diagram

```mermaid
flowchart TB
    subgraph Users
        Guest["👤 Guest"]
        Member["👥 Member"]
        Host["👑 Host"]
    end
    
    subgraph Authentication
        Register["📝 Register"]
        Login["🔐 Login"]
        Logout["🚪 Logout"]
    end
    
    subgraph Lobby_Management
        CreateLobby["➕ Create Lobby"]
        EditLobby["✏️ Edit Lobby"]
        DeleteLobby["🗑️ Delete Lobby"]
        CloseRecruit["🔒 Close Recruitment"]
        ViewApplicants["👀 View Applicants"]
        RecruitMember["✅ Recruit Member"]
        RejectMember["❌ Reject Member"]
    end
    
    subgraph Lobby_Interaction
        ViewFeed["📋 View Activity Feed"]
        ViewDetails["🔍 View Lobby Details"]
        ApplyLobby["📨 Apply to Lobby"]
        CancelApp["❌ Cancel Application"]
        VoteApplicant["🗳️ Vote on Applicant"]
        SendMessage["💬 Send Message"]
        ViewChat["💬 View Lobby Chat"]
    end
    
    subgraph Profile
        ViewProfile["👤 View Profile"]
        EditProfile["✏️ Edit Profile"]
        EndorseUser["⭐ Endorse User"]
    end
    
    Guest --> Register
    Guest --> Login
    Guest --> ViewFeed
    Guest --> ViewDetails
    
    Member --> Logout
    Member --> ViewFeed
    Member --> ViewDetails
    Member --> ApplyLobby
    Member --> CancelApp
    Member --> CreateLobby
    Member --> ViewProfile
    Member --> EditProfile
    Member --> EndorseUser
    Member --> VoteApplicant
    Member --> SendMessage
    Member --> ViewChat
    
    Host --> EditLobby
    Host --> DeleteLobby
    Host --> CloseRecruit
    Host --> ViewApplicants
    Host --> RecruitMember
    Host --> RejectMember
```

---

*Document Version: 1.0*  
*Last Updated: 2026-01-31*
