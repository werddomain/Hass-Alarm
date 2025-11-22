# Hass-Alarm Panel

![GitHub release](https://img.shields.io/github/v/release/werddomain/Hass-Alarm)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/werddomain/Hass-Alarm/build-addon.yml)
![License](https://img.shields.io/github/license/werddomain/Hass-Alarm)

Advanced alarm control panel for Home Assistant with PIN authentication, plugin support, and headless tablet interface.

## 🚀 Features

### Security
- 🔐 **BCrypt PIN Hashing** - Military-grade security with work factor 12
- 🛡️ **Rate Limiting** - Protection against brute force attacks (5 attempts, 15-min lockout)
- 🎫 **Session Management** - Secure 30-day sessions with auto-refresh
- 📍 **IP Tracking** - Full audit trail with IP address logging
- 🚫 **IP Filtering** - Optional IP range restrictions

### User Interface
- 📱 **Headless Panel** - Touch-optimized fullscreen interface for tablets
- 🖥️ **Admin Panel** - Complete user and PIN management
- ⌨️ **Standard Panel** - Traditional numeric keypad interface
- 🎨 **Color-Coded States** - Visual feedback (Green=Disarmed, Red=Armed, Orange=Armed Home)

### Integration
- 🏠 **Home Assistant Events** - Automatic event firing for notifications
- 🔌 **REST API** - Complete plugin architecture for custom integrations
- 📲 **Multi-Device Support** - Track unlimited tablets/devices
- 🔔 **Real-time Updates** - Live alarm state synchronization

### Technical
- 🐳 **Docker Ready** - Multi-architecture support (amd64, arm64, armv7, armhf, i386)
- 🏗️ **Home Assistant Add-on** - One-click installation
- 📊 **Database Support** - MySQL, SQLite
- ⚡ **High Performance** - <100ms response times, minimal resource usage

## 📦 Installation

### Home Assistant Add-on (Recommended)

1. Add this repository to your Home Assistant add-on store:
   ```
   https://github.com/werddomain/Hass-Alarm
   ```

2. Install the "Hass-Alarm Panel" add-on

3. Configure and start the add-on

**[📖 Full Installation Guide](INSTALLATION.md)**

### Docker

```bash
docker run -d \
  --name hass-alarm \
  -p 5000:80 \
  -e Ha__Host="your-ha-instance:8123/" \
  -e Ha__ApiKey="your_token" \
  -e Ha__Entities__Arm="input_boolean.alarm_home" \
  -e Ha__Entities__ArmHome="input_boolean.alarm_ar" \
  -e Admin__Email="admin@example.com" \
  -e Admin__Password="YourPassword123!" \
  ghcr.io/werddomain/hass-alarm:latest
```

### Manual Installation

See [INSTALLATION.md](INSTALLATION.md) for detailed instructions.

## 🎯 Quick Start

### Prerequisites

Create the required Home Assistant entities:

```yaml
# configuration.yaml
input_boolean:
  alarm_home:
    name: Alarm Armed
    icon: mdi:shield-lock

  alarm_ar:
    name: Alarm Armed Home
    icon: mdi:shield-home
```

### First-Time Setup

1. Access the admin panel at `http://homeassistant.local:8099`
2. Log in with your admin credentials
3. Create users under **Users** section
4. Assign PINs under **PIN Codes** section
5. Test the panel at `/Home/Panel` or `/HeadlessPanel`

## 📱 Usage

### Admin Panel
- **URL**: `http://homeassistant.local:8099`
- **Features**: User management, PIN management, action groups

### Standard Alarm Panel
- **URL**: `http://homeassistant.local:8099/Home/Panel`
- **Features**: Numeric keypad, PIN entry, alarm control

### Headless Tablet Panel
- **URL**: `http://homeassistant.local:8099/HeadlessPanel`
- **Features**:
  - Fullscreen touch interface
  - Device name registration
  - 30-day persistent sessions
  - Auto-lock after inactivity
  - Real-time state updates

### REST API
- **Base URL**: `http://homeassistant.local:8099/api/plugin`
- **Documentation**: [PLUGIN_GUIDE.md](PLUGIN_GUIDE.md)

## 🔌 API Example

```python
import requests

# Register device
device = requests.post("http://homeassistant.local:8099/api/plugin/device/register",
    json={"deviceName": "My Tablet"})

# Authenticate
auth = requests.post("http://homeassistant.local:8099/api/plugin/authenticate",
    json={"pin": "1234", "deviceId": device.json()["device_id"]})

# Change alarm state
requests.post("http://homeassistant.local:8099/api/plugin/state",
    headers={"X-Session-Token": auth.json()["session_token"]},
    json={"action": "arm"})
```

## 🏠 Home Assistant Integration

### Event Automation

```yaml
automation:
  - alias: "Alarm State Change Notification"
    trigger:
      platform: event
      event_type: hass_alarm_state_change
    action:
      - service: notify.mobile_app
        data:
          title: "Alarm Status Changed"
          message: >
            {{ trigger.event.data.friendly_name }} changed alarm to
            {{ trigger.event.data.new_state }} from
            {{ trigger.event.data.device_name }}
```

### Event Data

Events include:
- `timestamp` - ISO 8601 timestamp
- `user_name` - Username
- `friendly_name` - User's email or display name
- `pin_name` - Name assigned to the PIN
- `new_state` - New alarm state (arm, disarm, arm_home)
- `device_name` - Device that made the change
- `device_ip` - IP address of the device

## 📚 Documentation

- **[Installation Guide](INSTALLATION.md)** - Complete installation and deployment instructions
- **[Plugin Guide](PLUGIN_GUIDE.md)** - REST API documentation and examples
- **[Add-on Documentation](hass-alarm/DOCS.md)** - Home Assistant add-on specific docs
- **[Add-on README](hass-alarm/README.md)** - Add-on store information

## 🛠️ Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/werddomain/Hass-Alarm.git
cd Hass-Alarm/Hass-Alarm

# Restore dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Build
dotnet build

# Run
dotnet run
```

### Running Tests

```bash
dotnet test
```

### Building Docker Image

```bash
docker build -t hass-alarm -f hass-alarm/Dockerfile .
```

## 🔄 Version Management

### Automatic Releases

The project uses GitHub Actions for automatic builds and releases:

- Automatic builds on push to `main`
- Multi-architecture Docker images
- Semantic versioning
- Automated changelog generation

### Creating a Release

Use the GitHub Actions workflow:

1. Go to **Actions** → **Version Bump**
2. Click **Run workflow**
3. Select bump type (patch/minor/major)
4. The workflow will:
   - Update version in `config.json`
   - Create a git tag
   - Build multi-arch Docker images
   - Create GitHub release

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         Hass-Alarm                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │Admin Panel   │  │Standard Panel│  │Headless Panel│    │
│  │(Web UI)      │  │(Keypad)      │  │(Tablet UI)   │    │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘    │
│         │                  │                  │            │
│         └──────────────────┼──────────────────┘            │
│                            │                               │
│                    ┌───────▼────────┐                      │
│                    │   Controllers  │                      │
│                    │ - Home         │                      │
│                    │ - Plugin       │                      │
│                    │ - HeadlessPanel│                      │
│                    └───────┬────────┘                      │
│                            │                               │
│         ┌──────────────────┼──────────────────┐           │
│         │                  │                  │           │
│  ┌──────▼───────┐  ┌──────▼───────┐  ┌──────▼───────┐   │
│  │Session Service│ │AlarmState Svc│ │HA Event Service│   │
│  │- 30-day tokens│ │- State mgmt  │ │- Event firing  │   │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘   │
│         │                  │                  │           │
│         └──────────────────┼──────────────────┘           │
│                            │                               │
│                    ┌───────▼────────┐                      │
│                    │   Database     │                      │
│                    │ - Users/PINs   │                      │
│                    │ - Devices      │                      │
│                    │ - Sessions     │                      │
│                    └────────────────┘                      │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ API Calls
                             │
                    ┌────────▼─────────┐
                    │ Home Assistant   │
                    │ - input_boolean  │
                    │ - Events         │
                    └──────────────────┘
```

## 📊 Project Structure

```
Hass-Alarm/
├── .github/
│   └── workflows/          # GitHub Actions workflows
│       ├── build-addon.yml # Build and publish workflow
│       └── version-bump.yml# Version management workflow
├── hass-alarm/             # Home Assistant add-on
│   ├── rootfs/             # Add-on filesystem
│   │   └── etc/services.d/hass-alarm/
│   │       ├── run         # Startup script
│   │       └── finish      # Shutdown script
│   ├── config.json         # Add-on configuration
│   ├── Dockerfile          # Add-on Docker image
│   ├── README.md           # Add-on store page
│   └── DOCS.md             # Add-on documentation
├── Hass-Alarm/             # Main application
│   ├── Controllers/        # MVC controllers
│   │   ├── HomeController.cs
│   │   ├── PluginController.cs
│   │   └── HeadlessPanelController.cs
│   ├── Services/           # Business logic
│   │   ├── AlarmState.cs
│   │   ├── SessionService.cs
│   │   ├── HomeAssistantEventService.cs
│   │   ├── RateLimitService.cs
│   │   └── PinHashingService.cs
│   ├── Data/               # Data layer
│   │   ├── ApplicationDbContext.cs
│   │   └── Models/
│   │       ├── Device.cs
│   │       ├── PanelSession.cs
│   │       └── PinCode.cs
│   ├── Views/              # Razor views
│   │   ├── Home/
│   │   └── HeadlessPanel/
│   └── Migrations/         # EF Core migrations
├── INSTALLATION.md         # Installation guide
├── PLUGIN_GUIDE.md         # API documentation
├── README.md               # This file
└── repository.json         # Add-on repository config
```

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## 📝 License

MIT License - See [LICENSE](LICENSE) file for details

## 🙏 Acknowledgments

- [Home Assistant](https://www.home-assistant.io/) - Home automation platform
- [HADotNet](https://github.com/qJake/HADotNet) - Home Assistant .NET client
- [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net) - Password hashing

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/werddomain/Hass-Alarm/issues)
- **Documentation**: [Installation Guide](INSTALLATION.md) | [Plugin Guide](PLUGIN_GUIDE.md)
- **Discussions**: [GitHub Discussions](https://github.com/werddomain/Hass-Alarm/discussions)

## 🗺️ Roadmap

- [ ] Mobile app integration
- [ ] Biometric authentication support
- [ ] Multi-language support
- [ ] Custom themes
- [ ] Advanced reporting and analytics
- [ ] Integration with more alarm systems
- [ ] Voice assistant integration

## ⭐ Star History

If you find this project useful, please consider giving it a star!

---

Made with ❤️ for the Home Assistant community
