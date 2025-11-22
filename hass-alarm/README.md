# Hass-Alarm Panel - Home Assistant Add-on

![Supports aarch64 Architecture][aarch64-shield]
![Supports amd64 Architecture][amd64-shield]
![Supports armhf Architecture][armhf-shield]
![Supports armv7 Architecture][armv7-shield]
![Supports i386 Architecture][i386-shield]

Advanced alarm control panel with PIN authentication, plugin support, and headless tablet interface for Home Assistant.

## About

Hass-Alarm Panel is a comprehensive alarm control system that provides:

- **Secure PIN Authentication**: BCrypt-hashed PIN codes with rate limiting
- **Headless Tablet Interface**: Fullscreen panel optimized for wall-mounted tablets
- **REST API**: Complete plugin architecture for custom integrations
- **Session Management**: 30-day sessions that auto-refresh on activity
- **Home Assistant Events**: Automatic event firing for notifications and automations
- **Device Tracking**: Monitor which tablets/devices are accessing your alarm
- **Multi-user Support**: Individual PINs with friendly names for each user

## Features

### Security
- BCrypt PIN hashing (work factor 12)
- Rate limiting (5 attempts, 15-minute lockout)
- Session-based authentication with secure tokens
- IP address tracking and optional IP range filtering
- Full audit trail via Home Assistant events

### User Interface
- **Admin Panel**: Full-featured web interface for managing users and PINs
- **Headless Panel**: Touch-optimized interface for tablets (`/HeadlessPanel`)
- **Standard Panel**: Traditional keypad interface (`/Home/Panel`)
- Real-time alarm state updates
- Color-coded status indicators

### Integration
- Direct Home Assistant API integration
- Automatic event firing on alarm state changes
- REST API for third-party integrations
- Support for multiple alarm states (Disarm, Arm, Arm Home)

## Installation

1. Add this repository to your Home Assistant add-on store:
   - Navigate to **Supervisor** → **Add-on Store**
   - Click the **⋮** menu in the top right
   - Select **Repositories**
   - Add: `https://github.com/werddomain/Hass-Alarm`

2. Find "Hass-Alarm Panel" in the add-on store and click **Install**

3. Configure the add-on (see Configuration section)

4. Start the add-on

5. Access the panel:
   - Admin interface: `http://homeassistant.local:8099`
   - Headless panel: `http://homeassistant.local:8099/HeadlessPanel`

## Configuration

### Required Configuration

```yaml
ha_host: "homeassistant.local:8123"
ha_use_ssl: false
entities:
  arm: "input_boolean.alarm_home"
  arm_home: "input_boolean.alarm_ar"
admin:
  email: "admin@hassalarm.local"
  password: "ChangeMe123!"
security:
  allowed_ip_range: ""
  enable_rate_limiting: true
```

### Configuration Options

| Option | Type | Required | Description |
|--------|------|----------|-------------|
| `ha_host` | string | Yes | Home Assistant host and port |
| `ha_use_ssl` | boolean | Yes | Use HTTPS for Home Assistant connection |
| `entities.arm` | string | Yes | Entity ID for arm state |
| `entities.arm_home` | string | Yes | Entity ID for arm home state |
| `admin.email` | string | Yes | Admin user email |
| `admin.password` | string | Yes | Admin user password (change this!) |
| `security.allowed_ip_range` | string | No | IP range filter (e.g., "192.168.1.1-192.168.1.255") |
| `security.enable_rate_limiting` | boolean | Yes | Enable rate limiting for PIN attempts |

### Home Assistant Setup

Before using this add-on, you need to create the required input_boolean entities in Home Assistant:

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

After adding these, restart Home Assistant.

## Usage

### First-Time Setup

1. Access the admin interface at `http://homeassistant.local:8099`
2. Log in with the admin credentials from your configuration
3. Navigate to **Users** to create user accounts
4. Navigate to **PIN Codes** to assign PINs to users
5. Test the alarm panel at `/Home/Panel` or `/HeadlessPanel`

### Headless Tablet Setup

For wall-mounted tablets:

1. Open a browser on the tablet
2. Navigate to `http://homeassistant.local:8099/HeadlessPanel`
3. Enter a device name (e.g., "Kitchen Tablet")
4. Enter your PIN
5. The session will persist for 30 days
6. Set the browser to fullscreen mode
7. Optionally use kiosk mode apps for locked-down tablets

### API Usage

See the [Plugin Guide](https://github.com/werddomain/Hass-Alarm/blob/main/PLUGIN_GUIDE.md) for complete API documentation.

Quick example:
```bash
# Register device
curl -X POST http://homeassistant.local:8099/api/plugin/device/register \
  -H "Content-Type: application/json" \
  -d '{"deviceName":"My Device"}'

# Authenticate
curl -X POST http://homeassistant.local:8099/api/plugin/authenticate \
  -H "Content-Type: application/json" \
  -d '{"pin":"1234","deviceId":1}'
```

## Home Assistant Automations

The add-on fires events to Home Assistant when alarm states change:

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

Event data includes:
- `timestamp` - When the change occurred
- `user_name` - Username who made the change
- `friendly_name` - User's email or friendly name
- `pin_name` - Name assigned to the PIN
- `new_state` - New alarm state (arm, disarm, arm_home)
- `device_name` - Name of the device/tablet used
- `device_ip` - IP address of the device

## Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/werddomain/Hass-Alarm/issues
- Documentation: https://github.com/werddomain/Hass-Alarm

## License

MIT License - See LICENSE file for details

[aarch64-shield]: https://img.shields.io/badge/aarch64-yes-green.svg
[amd64-shield]: https://img.shields.io/badge/amd64-yes-green.svg
[armhf-shield]: https://img.shields.io/badge/armhf-yes-green.svg
[armv7-shield]: https://img.shields.io/badge/armv7-yes-green.svg
[i386-shield]: https://img.shields.io/badge/i386-yes-green.svg
