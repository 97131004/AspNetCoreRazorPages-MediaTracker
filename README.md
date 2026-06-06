# ASP.NET Core / Razor Pages : Media Tracker

Media Tracker is a ASP.NET Core web application designed to centralize the tracking of watched and upcoming movies, TV shows and video games. The application features a single-dashboard interface to manage watch and play histories, track upcoming releases, and rate entries. User can attach arbitrary key-value metadata - such as personal notes, links, or custom text fields - to any media item. Using Razor Pages (+ View Components) for frontend and EF Core (SQL server) to store media entries.

## Screenshots

![screenshot](Screenshots/1.png?raw=true)

![screenshot](Screenshots/2.png?raw=true)

![screenshot](Screenshots/3.png?raw=true)

## Technologies & Versions

- **Backend:** ASP.NET Core (.NET 8.0)
- **Frontend:** Razor Pages, View Components
- **Database:** MS SQL Server 2022 (LocalDB)
- **ORM:** Entity Framework Core 9.0.0

## Architecture

This application is built as a **Modular Monolith** to enforce strict module separation and decoupling:

- **Modules/Media:** Contains models, enums, `MediaDbContext`, and `MediaService` for library management.
- **Modules/MediaData:** Contains `MediaDataDbContext` and `MediaDataService` for storing dynamic metadata associated with media entries.
- **Decoupling & Isolation:** Both modules target the same database but use completely independent EF Core DbContexts, ensuring no direct database-level dependency exists between the modules.
- **Frontend/UI:** Uses the standard Razor Pages (PageModel) paired with ViewComponents (`AddMedia` and `MediaDetail`) as components for modals. Interaction fully relies on Razor Pages' server-side rendering. This approach somewhat resembles classic WebForms, but is completely stateless per request. While this approach increases development complexity, it could improve search engine optimization (SEO) in the future.

## Features

- **Unified Library Tracking:** Manage the tracking of watched and upcoming movies, TV shows, and video games in one dashboard.
- **Sorting:** Ability to sort any media entry by title, type, release date, status, rating.
- **Dynamic Metadata:** Attach custom attributes (links, developer, platform, etc.) to media entries. URLs are auto-converted to clickable hyperlinks.
- **Inline Editing:** In-place updates for title, release date, rating, status and key-value details/metadata.
- **Priority Watch/Play List:** Landing page sorts upcoming planned items ascendingly and past releases descendingly.
- **Deletion:** Removing a media entry automatically cleans up all associated metadata.

## Database Structure

Database schema explicitly separates core entity metadata (MediaEntries) from its dynamic attributes (MediaDataEntries). This decoupled design strictly enforces a modular monolith pattern, ensuring easy decoupling and extraction into an independent microservice in the future.

![screenshot](Screenshots/6.png?raw=true)

## Installation & Setup

Build the solution once, then run following commands to initialise database (for main project):

   ```bash
   dotnet ef migrations add InitialCreate --context MediaDbContext
   dotnet ef migrations add InitialCreate --context MediaDataDbContext
   dotnet ef database update --context MediaDbContext
   dotnet ef database update --context MediaDataDbContext
   ```

Database connection string in `appsettings.json`.

## Screenshots

![screenshot](Screenshots/4.png?raw=true)

![screenshot](Screenshots/5.png?raw=true)

![screenshot](Screenshots/7.png?raw=true)

## License

MIT
