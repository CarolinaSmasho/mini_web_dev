```mermaid
classDiagram
    class User {
        +string userId
        +string username
        +string email
        +string password
        +string avatar
        +string bio
        +float karmaScore
        +Karma[] karmaHistory
        +string[] vibeTags
        +string[] gameLibrary
        +string[] friend_id
        +string[] social_media
        +Notification[] notifications
        -datetime createdAt
        +register()
        +login()
        +logout()
        +updateProfile()
        +updateFriend()
        +endorse(User targetUser)
    }

    class Lobby {
        +string lobbyId
        +string title
        +string game
        +User[] id_pending
        +User[] id_confirm
        +string description
        +int currentPlayers
        +int maxPlayers
        +string[] moods
        +string[] roles
        +bool isRecruiting
        +datetime scheduledTime
        +datetime createdAt
        +create()
        +update()
        +delete()
        +openRecruitment()
        +closeRecruitment()
        +getMissingRoles()
        +confirm()
    }

    class Karma {
        +string id
        +float score
        +datetime date
        +string comment
    }

    class Notification {
        +string text
        +datetime date
        +bool isRead
    }

    %% Relationships
    User "1" --> "*" Karma : has
    User "1" --> "*" Notification : receives
    User "1" -- "*" User : friends
    Lobby "1" o-- "*" User : contains