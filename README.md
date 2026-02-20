# InstaClone

A proof-of-concept Instagram clone built with .NET 9 Minimal APIs, Vue.js, PostgreSQL, and Docker Compose. The goal is a `docker compose up` experience that gives you a fully working app with hot-reload — edit code on your Mac, see changes instantly in the containers.

## Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│  Docker Compose                                                      │
│                                                                      │
│  ┌─────────────────┐  ┌──────────────────┐  ┌────────────────────┐  │
│  │  web             │  │  api              │  │  db                │  │
│  │  Node 22         │  │  .NET 9 SDK       │  │  PostgreSQL 16     │  │
│  │                  │  │                   │  │                    │  │
│  │  Vite dev server │  │  dotnet watch run │  │  :5432             │  │
│  │  (hot-reload)    │─→│  (hot-reload)     │─→│                    │  │
│  │                  │  │                   │  │  Volume: pgdata    │  │
│  │  Proxy: /api     │  │  Uploads: bind    │  └────────────────────┘  │
│  │         /uploads │  │  Source: bind     │                          │
│  └────────┬─────────┘  └────────┬──────────┘                          │
│           │ :4000               │ :8080                               │
└───────────┼─────────────────────┼────────────────────────────────────┘
            │                     │
   http://localhost:4000    http://localhost:8080
   (Frontend)               (API + Scalar docs)
```

**Key decisions:**

- **SDK image** (not aspnet runtime) — full build toolchain for `dotnet watch` hot-reload
- **Polling file watcher** — both Vite and `dotnet watch` use polling because inotify doesn't work across Docker bind mounts on macOS
- **Vite proxy** — the frontend proxies `/api` and `/uploads` to the API container, avoiding CORS configuration
- **Anonymous volumes for bin/obj/node_modules** — prevents Linux container binaries from conflicting with macOS host
- **DB healthcheck + depends_on** — API waits for PostgreSQL to be ready before starting
- **EnsureCreated()** — auto-creates schema on startup (no migrations needed for a POC)
- **Scalar.AspNetCore** — replaces Swagger UI (.NET 9 dropped Swashbuckle from templates)

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|:----:|-------------|
| POST | `/api/auth/register` | | Create account |
| POST | `/api/auth/login` | | Get JWT token |
| POST | `/api/posts` | JWT | Create post with image upload |
| GET | `/api/posts/{id}` | | Get single post |
| GET | `/api/posts/feed?page=1&pageSize=10` | | Paginated feed (newest first) |
| POST | `/api/posts/{id}/comments` | JWT | Add comment |
| GET | `/api/posts/{id}/comments` | | List comments |
| POST | `/api/posts/{id}/likes` | JWT | Like post |
| DELETE | `/api/posts/{id}/likes` | JWT | Unlike post |

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

That's it. No .NET SDK, no Node.js, no PostgreSQL, no other tools needed on your machine.

## Running

```bash
docker compose up
```

First run pulls images and installs dependencies (~2-3 min). Subsequent starts are fast.

Once you see these in the logs, the app is ready:

```
api-1  | Now listening on: http://[::]:8080
web-1  | VITE v6.x.x  ready in xxx ms
```

**Access points:**

- Frontend: http://localhost:4000
- API: http://localhost:8080
- Scalar API docs: http://localhost:8080/scalar/v1

**Other commands:**

```bash
docker compose up -d     # run in background
docker compose logs -f   # tail logs
docker compose down      # stop (data persists)
docker compose down -v   # stop and wipe everything (fresh start)
```

## Frontend

The frontend is a Vue 3 single-page application with Tailwind CSS v4.

**Pages:**

| Route | Description |
|-------|-------------|
| `/` | Feed — scrollable posts with Load More pagination |
| `/login` | Log in with email and password |
| `/register` | Create a new account |
| `/posts/create` | Upload an image with a caption (requires login) |
| `/posts/:id` | Post detail with comments, like/unlike |

**Stack:** Vue 3 (Composition API + `<script setup>`), Vue Router, Vite, Tailwind CSS v4

**How it works:**
- JWT token stored in localStorage, attached to API requests automatically
- Vite dev server proxies `/api` and `/uploads` to the backend — the browser only talks to one origin
- No state management library — a simple reactive composable (`useAuth`) shares auth state across components

## Seed Data

The database is automatically seeded on first startup with sample data so you can start testing immediately.

**Users** (all with password `password`):

| Username | Email |
|----------|-------|
| alice | alice@example.com |
| bob | bob@example.com |
| charlie | charlie@example.com |

**Content:** 6 posts with placeholder images, 8 comments, and 11 likes spread across users.

The seeder is idempotent — it only runs when the Users table is empty. Use `docker compose down -v` to reset.

## Testing with curl

### 1. Login

```bash
# Login as alice (or bob, charlie — all use "password")
curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"password"}'
```

Response:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": "019c79ea-0e85-78db-b189-3701f14b347c",
  "username": "alice"
}
```

Save the token for authenticated requests:

```bash
TOKEN="eyJhbGciOiJIUzI1NiIs..."
```

### 2. Browse the Feed

```bash
curl -s http://localhost:8080/api/posts/feed | python3 -m json.tool
```

With pagination:

```bash
curl -s "http://localhost:8080/api/posts/feed?page=1&pageSize=2" | python3 -m json.tool
```

### 3. Get a Single Post

Copy any `id` from the feed response:

```bash
curl -s http://localhost:8080/api/posts/{post-id} | python3 -m json.tool
```

### 4. Create a Post

```bash
curl -s -X POST http://localhost:8080/api/posts \
  -H "Authorization: Bearer $TOKEN" \
  -F "image=@test-data/photos/1.jpg" \
  -F "caption=Hello from curl" | python3 -m json.tool
```

### 5. Add a Comment

```bash
curl -s -X POST http://localhost:8080/api/posts/{post-id}/comments \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"text":"Nice photo"}' | python3 -m json.tool
```

### 6. List Comments

```bash
curl -s http://localhost:8080/api/posts/{post-id}/comments | python3 -m json.tool
```

### 7. Like / Unlike a Post

```bash
# Like
curl -s -X POST http://localhost:8080/api/posts/{post-id}/likes \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool

# Unlike
curl -s -X DELETE http://localhost:8080/api/posts/{post-id}/likes \
  -H "Authorization: Bearer $TOKEN" | python3 -m json.tool
```

### 8. Register a New User

```bash
curl -s -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"newuser","email":"new@example.com","password":"mypassword","displayName":"New User"}' \
  | python3 -m json.tool
```

## Hot-Reload

**Backend:** Edit any `.cs` file under `backend/` on your host machine. The container detects the change via polling and automatically rebuilds and restarts the API.

```
api-1  | dotnet watch ⌚ File changed: /app/src/...
api-1  | dotnet watch 🔥 Hot reload of changes succeeded.
```

**Frontend:** Edit any `.vue`, `.js`, or `.css` file under `frontend/`. Vite's HMR updates the browser instantly without a full page reload.

## Project Structure

```
poc-dotnet/
├── docker-compose.yml
├── uploads/                        # Bind-mounted image storage
├── backend/                        # .NET 9 Minimal API
│   ├── InstaClone.Api.csproj
│   ├── Program.cs                  # App setup, middleware, endpoint registration
│   ├── appsettings.json
│   ├── Data/
│   │   ├── AppDbContext.cs          # EF Core context + model config
│   │   └── DbSeeder.cs             # Seed data (3 users, 6 posts, comments, likes)
│   ├── Models/
│   │   └── User.cs, Post.cs, Comment.cs, Like.cs
│   ├── Dtos/
│   │   └── AuthDtos.cs, PostDtos.cs, CommentDtos.cs
│   ├── Services/
│   │   ├── TokenService.cs          # JWT generation (HS256)
│   │   └── ImageService.cs          # File upload validation + storage
│   └── Endpoints/
│       ├── AuthEndpoints.cs         # Register, Login
│       ├── PostEndpoints.cs         # Create, Get, Feed
│       ├── CommentEndpoints.cs      # Add, List
│       └── LikeEndpoints.cs         # Like, Unlike
└── frontend/                        # Vue.js SPA
    ├── Dockerfile.dev
    ├── package.json
    ├── vite.config.js               # Dev server + API proxy config
    ├── index.html
    └── src/
        ├── main.js
        ├── App.vue
        ├── router.js
        ├── style.css                # Tailwind CSS v4 entry point
        ├── composables/
        │   ├── useAuth.js           # Reactive auth state (token, user)
        │   └── useApi.js            # Fetch wrapper with JWT + FormData
        ├── views/
        │   ├── FeedView.vue         # Post feed with Load More
        │   ├── LoginView.vue        # Login form
        │   ├── RegisterView.vue     # Registration form
        │   ├── CreatePostView.vue   # Image upload + caption
        │   └── PostDetailView.vue   # Single post, comments, likes
        └── components/
            ├── NavBar.vue           # Top nav with auth controls
            ├── PostCard.vue         # Post card for feed
            └── CommentList.vue      # Comment list
```
