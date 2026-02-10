# Design System: Gamer LFG
**Project ID:** 4667465622508173432
**Theme:** Dark Gaming with Orange Accent (Pornhub-style)

## 1. Visual Theme & Atmosphere

The Gamer LFG interface embodies a **dark, immersive gaming aesthetic** with bold orange accents. The atmosphere is dense yet navigable, with deep shadows punctuated by vibrant orange highlights. The design evokes late-night gaming sessions—energetic, bold, and community-driven. Glassmorphism effects add depth and modernity, creating floating card-like elements that feel premium and interactive.

## 2. Color Palette & Roles

| Name | Hex | Role |
|------|-----|------|
| **Obsidian Black** | `#0D0D0D` | Primary background, provides depth |
| **Midnight Navy** | `#1A1A2E` | Secondary background, card surfaces |
| **Primary Orange** | `#FF9900` | Primary accent, buttons, highlights |
| **Electric Blue** | `#00D9FF` | Secondary accent, links, interactive states |
| **Toxic Green** | `#00FF88` | Success states, live indicators, online status |
| **Crimson Red** | `#FF4757` | Error states, warnings, reject actions |
| **Ghost White** | `#E8E8E8` | Primary text on dark backgrounds |
| **Smoke Gray** | `#6B6B6B` | Secondary text, muted labels |
| **Golden Glow** | `#FFD93D` | Host badges, premium highlights, karma stars |

## 3. Typography Rules

- **Font Family**: Inter, system-ui, sans-serif (clean, modern gaming aesthetic)
- **Headings**: Bold (700), tight letter-spacing (-0.02em), larger sizes
- **Body Text**: Regular (400), comfortable line-height (1.6)
- **Accent Text**: Medium (500), used for labels and tags
- **Monospace**: JetBrains Mono for technical elements (game names, IDs)

## 4. Component Stylings

* **Buttons**: 
  - Primary: Pill-shaped with Primary Orange background, white text, subtle glow on hover
  - Secondary: Ghost buttons with Electric Blue border, transparent background
  - Danger: Crimson Red with white text for destructive actions

* **Cards/Containers**: 
  - Glassmorphism effect with semi-transparent Midnight Navy background
  - Subtle backdrop blur (8-12px)
  - Gently rounded corners (12-16px border-radius)
  - Thin border with 10% white opacity for edge definition
  - Soft shadow for floating effect

* **Inputs/Forms**: 
  - Dark background (slightly lighter than Obsidian)
  - Subtle border, Electric Blue glow on focus
  - Placeholder text in Smoke Gray

* **Tags/Badges**:
  - Small pill shapes with semi-transparent backgrounds
  - Color-coded by type (mood tags, role tags, status badges)

* **Navigation**:
  - Sticky header with glassmorphism
  - Logo on left, nav links centered or right
  - User avatar with dropdown on far right

## 5. Layout Principles

- **Generous whitespace** between major sections
- **Card-based layout** for lobby listings and user profiles
- **Grid system**: 3-4 column layouts on desktop, stack on mobile
- **Consistent padding**: 16px minimum, 24px standard, 32px large sections
- **Max content width**: 1200px with centered alignment

## 6. Design System Notes for Stitch Generation

**REQUIRED IN ALL PROMPTS:**

```
DESIGN SYSTEM:
- Theme: Dark gaming aesthetic with bold orange accents
- Background: Deep obsidian black (#0D0D0D) with midnight navy (#1A1A2E) cards
- Primary accent: Primary orange (#FF9900) for buttons and highlights  
- Secondary accent: Electric blue (#00D9FF) for links and interactions
- Success/Live: Toxic green (#00FF88)
- Error/Reject: Crimson red (#FF4757)
- Text: Ghost white (#E8E8E8) on dark, smoke gray (#6B6B6B) for muted
- Effects: Glassmorphism with backdrop blur, subtle glow effects on hover
- Corners: Generously rounded (12-16px)
- Typography: Inter font family, bold headings
- Layout: Card-based, generous spacing, max-width 1200px
```
