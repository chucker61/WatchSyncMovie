# WatchSync - Synchronized Movie Watching Application

WatchSync is a real-time movie watching application that allows friends to watch movies and series together in perfect synchronization using SignalR technology.

## Features

### 🎬 Synchronized Playback
- Real-time synchronization of play, pause, and seek operations
- Automatic sync when joining an ongoing session
- Host controls for managing the viewing experience

### 🏠 Room Management
- Create private rooms with optional password protection
- Join existing rooms with Room ID
- Real-time user management and notifications

### 💬 Live Chat
- Real-time chat during movie watching
- User-friendly chat interface
- Message history for the session

### 🎮 Modern Video Player
- Custom HTML5 video player with modern controls
- Full-screen support
- Volume controls and progress bar
- Keyboard shortcuts (Space for play/pause)

### 📱 Responsive Design
- Works on desktop, tablet, and mobile devices
- Modern gradient UI design
- Intuitive user interface

## Technology Stack

### Backend (.NET 9)
- **ASP.NET Core Web API** - REST API endpoints
- **SignalR** - Real-time communication
- **In-Memory Storage** - Room and movie management
- **Controllers** - Movie and room management APIs

### Frontend (Blazor WebAssembly)
- **Blazor WebAssembly** - Modern web framework
- **SignalR Client** - Real-time communication
- **JavaScript Interop** - Custom video player
- **CSS Grid & Flexbox** - Responsive layouts
- **Font Awesome** - Modern icons

## Getting Started

### Prerequisites
- .NET 9 SDK
- Modern web browser with WebAssembly support

### Running the Application

1. **Start the Server**
   ```bash
   cd WatchSyncMovie.Server
   dotnet run
   ```
   The server will start on `https://localhost:7083` (HTTPS) or `http://localhost:5172` (HTTP)

2. **Start the Client**
   ```bash
   cd WatchSyncMovie.Client
   dotnet run
   ```
   The client will start on `https://localhost:7114` (HTTPS) or `http://localhost:5281` (HTTP)

3. **Access the Application**
   Open your browser and navigate to `https://localhost:7114` or `http://localhost:5281`

**Note:** The client is configured to communicate with the server at `https://localhost:7083`

## How to Use

### Creating a Room
1. Visit the home page
2. Click "Create Room"
3. Enter your username
4. Optionally set a password for private rooms
5. Click "Create Room"

### Joining a Room
1. Get the Room ID from the room host
2. Enter the Room ID on the home page
3. Click "Join Room"
4. Enter your username and password (if required)
5. Click "Join Room"

### Adding Movies
Room hosts can add movies in two ways:

**From URL:**
1. As a room host, click "Add Movie" in the host controls
2. Select "From URL" tab
3. Enter the movie title and video URL
4. Optionally add a description
5. Click "Add Movie"

**Upload File:**
1. As a room host, click "Add Movie" in the host controls
2. Select "Upload File" tab
3. Enter the movie title
4. Choose a video file (MP4, WebM, AVI, MOV, MKV supported)
5. Optionally add a description
6. Click "Upload Movie"

**Note:** Uploaded files are limited to 500MB and are stored on the server.

### Selecting Movies
1. As a room host, click "Select Movie"
2. Choose from available movies (includes both uploaded files and URL-based movies)
3. The system automatically scans `/wwwroot/videos/` folder for additional video files
4. The movie will be synchronized for all users

**Note:** The movie list automatically includes:
- Movies added via URL
- Movies uploaded through the web interface  
- Video files placed directly in the server's `/wwwroot/videos/` folder

**Technical Details:**
- URL-based movies: Stored in memory with custom IDs
- Uploaded movies: Stored in memory + physical file with GUID filenames
- Direct files: Automatically scanned and assigned `wwwroot_` prefixed IDs
- All movie types are accessible through the same "Select Movie" interface

### Video Controls
- **Space Bar**: Play/Pause toggle (synchronized with all users)
- **Play/Pause Button**: Control playback (synchronized with all users)
- **Click Progress Bar**: Seek to position (synchronized with all users)
- **Volume Controls**: Adjust audio (local only)
- **Fullscreen Button**: Toggle fullscreen mode (local only)

**Note:** All playback controls (play, pause, seek) are automatically synchronized across all users in the room through SignalR.

## API Endpoints

### Movies API
- `GET /api/movies` - Get all movies
- `GET /api/movies/{id}` - Get specific movie
- `POST /api/movies` - Create new movie
- `POST /api/movies/from-url` - Create movie from URL
- `POST /api/movies/upload` - Upload movie file (multipart/form-data)
- `DELETE /api/movies/{id}` - Delete movie (also deletes uploaded files)

### Rooms API
- `GET /api/rooms` - Get active rooms
- `GET /api/rooms/{id}` - Get specific room

### SignalR Hub
- **Endpoint**: `/movieSyncHub`
- **Methods**: JoinRoom, CreateRoom, Play, Pause, Seek, ChangeMovie, SendMessage

## Supported Video Formats

**For URL-based videos:**
- MP4 (H.264/H.265)
- WebM
- Direct video URLs
- Most modern video formats supported by HTML5

**For uploaded files:**
- MP4 (recommended)
- WebM
- AVI
- MOV
- MKV
- OGG

**File Limitations:**
- Maximum file size: 1GB (via web upload)
- Files are stored in `/wwwroot/videos/` on server
- Automatic file cleanup when movies are deleted via web interface
- Files placed directly in `/wwwroot/videos/` are automatically discovered and shown in movie list

## Architecture

### Server Components
```
WatchSyncMovie.Server/
├── Controllers/           # API Controllers
│   ├── MoviesController   # Movie management
│   └── RoomsController    # Room management
├── Hubs/                  # SignalR Hubs
│   └── MovieSyncHub       # Real-time communication
├── Models/                # Data models
│   ├── Movie             # Movie entity
│   ├── Room              # Room entity
│   ├── User              # User entity
│   └── SyncEvent         # Synchronization events
├── Services/             # Business logic
│   ├── IMovieService     # Movie service interface
│   ├── MovieService      # Movie service implementation
│   ├── IRoomService      # Room service interface
│   └── RoomService       # Room service implementation
└── Program.cs            # Application configuration
```

### Client Components
```
WatchSyncMovie.Client/
├── Pages/                # Razor pages
│   ├── Home              # Landing page
│   └── MovieRoom         # Main movie room
├── Layout/               # Layout components
│   ├── MainLayout        # Main layout
│   └── NavMenu           # Navigation menu
├── Models/               # Client-side models
│   ├── Movie            # Movie model
│   ├── Room             # Room model
│   └── User             # User model
├── Services/            # Client services
│   ├── SignalRService   # SignalR communication
│   └── VideoPlayerService # Video player management
└── wwwroot/             # Static files
    ├── css/             # Stylesheets
    └── js/              # JavaScript files
```

## Security Considerations

- Room passwords are transmitted in plain text (consider encryption for production)
- No user authentication system (basic username-based identification)
- CORS configured for development (adjust for production)
- Input validation on both client and server sides

## Future Enhancements

- [ ] User authentication and profiles
- [x] Movie upload functionality ✅
- [x] Automatic video file discovery ✅
- [ ] Persistent storage (database)
- [ ] Video quality selection
- [ ] Video thumbnail generation for uploads
- [ ] Bulk file upload
- [ ] Video metadata extraction (duration, resolution)
- [ ] File organization (folders, categories)
- [ ] Admin interface for file management
- [ ] Screen sharing capabilities
- [ ] Mobile app development
- [ ] Advanced chat features (emojis, reactions)
- [ ] Room moderation tools
- [ ] Watch history and favorites
- [ ] Cloud storage integration (AWS S3, Azure Blob)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is open source and available under the MIT License.

## Support

For questions and support, please create an issue in the repository.

---

**Enjoy watching movies together with WatchSync! 🎬🍿** 