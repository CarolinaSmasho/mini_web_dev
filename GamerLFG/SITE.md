# SITE.md - Gamer LFG

## 1. Vision

Gamer LFG is a web application for finding gaming partners (Looking For Group). It connects gamers who want to find the right party for their play style, with features like lobby creation, squad voting, and in-lobby chat.

## 2. Tech Stack

- **Backend**: ASP.NET Core MVC (.NET 8)
- **Frontend**: Razor Views + Stitch-generated UI
- **Database**: Firebase Firestore
- **Auth**: Session-based authentication

## 3. Stitch Project

- **Project ID**: 13747810667632561170
- **Title**: Gamer LFG

## 4. Sitemap

- [ ] `_Layout.cshtml` - Main layout shell (header, nav, footer)
- [ ] `Home/Index.cshtml` - Activity Feed (My Lobbies, Pending, Find Squad)
- [ ] `Auth/Login.cshtml` - Login page
- [ ] `Auth/Register.cshtml` - Registration page
- [ ] `Lobby/Index.cshtml` - Lobby list/browse
- [ ] `Lobby/Create.cshtml` - Create lobby form
- [ ] `Lobby/Details.cshtml` - Lobby detail (chat, squad, applicants)
- [ ] `User/Profile.cshtml` - User profile page

## 5. Roadmap (In Order)

1. Generate main layout with header/nav/footer
2. Generate Activity Feed (Home page)
3. Generate Auth pages (Login + Register)
4. Generate Lobby management pages
5. Generate Profile page

## 6. Creative Freedom

- Add visual effects for live lobbies (pulsing indicators)
- Karma score stars with golden glow
- Mood tags with color coding
- Squad voting visualization
