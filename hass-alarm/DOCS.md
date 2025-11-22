# Hass-Alarm Panel Documentation

## Installation

1. Add this repository URL to your Home Assistant Add-on Store
2. Install the "Hass-Alarm Panel" add-on
3. Configure the add-on (see Configuration below)
4. Start the add-on
5. Check the logs to ensure it started successfully

## Prerequisites

Before installing this add-on, you must create the required input_boolean entities in your Home Assistant configuration:

```yaml
# Add to configuration.yaml
input_boolean:
  alarm_home:
    name: Alarm Armed
    icon: mdi:shield-lock

  alarm_ar:
    name: Alarm Armed Home
    icon: mdi:shield-home
```

After adding these entities, restart Home Assistant before starting the add-on.

## Configuration

### Basic Configuration

The add-on requires the following basic configuration:

```yaml
ha_host: "homeassistant.local:8123"
ha_use_ssl: false
entities:
  arm: "input_boolean.alarm_home"
  arm_home: "input_boolean.alarm_ar"
admin:
  email: "admin@example.com"
  password: "YourSecurePassword123!"
security:
  allowed_ip_range: ""
  enable_rate_limiting: true
```

### Configuration Options Explained

#### Home Assistant Connection

- **ha_host**: The hostname and port of your Home Assistant instance
  - Example: `homeassistant.local:8123`
  - If using HTTPS, set `ha_use_ssl: true`
  - The add-on automatically uses the Supervisor token for authentication

- **ha_use_ssl**: Whether to use HTTPS for the connection
  - Set to `true` if your Home Assistant uses SSL
  - Default: `false`

#### Entities

- **entities.arm**: The input_boolean entity for the "Armed" state
  - Must exist in your Home Assistant configuration
  - Example: `input_boolean.alarm_home`

- **entities.arm_home**: The input_boolean entity for the "Armed Home" state
  - Must exist in your Home Assistant configuration
  - Example: `input_boolean.alarm_ar`

#### Admin Account

- **admin.email**: Email address for the admin account
  - Used to log in to the admin panel
  - Can be any valid email format

- **admin.password**: Password for the admin account
  - **IMPORTANT**: Change this from the default!
  - Must be strong (uppercase, lowercase, numbers, symbols)
  - Minimum 8 characters recommended

#### Security

- **security.allowed_ip_range**: Optional IP range filter
  - Restricts access to specific IP addresses
  - Format: `"192.168.1.1-192.168.1.255"`
  - Leave empty (`""`) to allow all IPs
  - Useful for limiting access to your local network

- **security.enable_rate_limiting**: Enable/disable rate limiting
  - When enabled, limits failed PIN attempts to 5 per IP
  - After 5 failed attempts, IP is locked out for 15 minutes
  - **Recommended**: Keep this enabled for security

## First-Time Setup

After starting the add-on:

1. **Access the Admin Panel**
   - Open `http://homeassistant.local:8099` in your browser
   - Or click "Open Web UI" in the add-on page
   - Log in with your admin email and password

2. **Create Users**
   - Navigate to the Users section
   - Click "Create New User"
   - Fill in user details (username, email, password)
   - Assign appropriate role (Admin, Manager, or Member)

3. **Assign PIN Codes**
   - Navigate to PIN Codes section
   - Click "Create PIN"
   - Select a user
   - Enter a 4-8 digit PIN
   - Give the PIN a friendly name (e.g., "John's PIN")
   - Enable the PIN

4. **Test the Panel**
   - Navigate to `/Home/Panel` or `/HeadlessPanel`
   - Enter the PIN you created
   - Verify you can control the alarm states

## Usage

### Admin Panel

Access at: `http://homeassistant.local:8099`

Features:
- User management (create, edit, delete users)
- PIN code management (assign, enable/disable, migrate)
- Action groups (for advanced automation)
- Full audit capabilities

### Standard Alarm Panel

Access at: `http://homeassistant.local:8099/Home/Panel`

Features:
- Traditional numeric keypad interface
- PIN entry for authentication
- Control alarm states (Arm, Disarm, Arm Home)
- Auto-lock after 30 seconds of inactivity

### Headless Tablet Panel

Access at: `http://homeassistant.local:8099/HeadlessPanel`

Optimized for wall-mounted tablets:
- Fullscreen, touch-optimized interface
- Large buttons and clear visual feedback
- Device name registration for tracking
- 30-day persistent sessions
- Real-time state updates every 5 seconds
- Perfect for dedicated alarm panels

**Tablet Setup Tips:**
1. Use fullscreen mode (F11 in most browsers)
2. Consider using a kiosk mode app to prevent navigation
3. Enable "Stay Awake" mode on Android tablets
4. Set screen timeout to maximum or disable
5. Consider using Fully Kiosk Browser for advanced kiosk features

### REST API

Access at: `http://homeassistant.local:8099/api/plugin/*`

The add-on provides a complete REST API for custom integrations. See the [Plugin Guide](https://github.com/werddomain/Hass-Alarm/blob/main/PLUGIN_GUIDE.md) for full API documentation.

## Home Assistant Integration

### Event Automation

The add-on fires events to Home Assistant whenever the alarm state changes. Use these for notifications and automations:

```yaml
automation:
  - alias: "Send notification on alarm state change"
    trigger:
      platform: event
      event_type: hass_alarm_state_change
    action:
      - service: notify.mobile_app_your_phone
        data:
          title: "Alarm Status Changed"
          message: >
            {{ trigger.event.data.friendly_name }} changed the alarm to
            {{ trigger.event.data.new_state }} using
            {{ trigger.event.data.device_name }}
          data:
            push:
              category: "alarm"
```

### Available Event Data

Events include the following data:

| Field | Description | Example |
|-------|-------------|---------|
| `timestamp` | ISO 8601 timestamp | "2025-11-22T12:00:00.000Z" |
| `user_name` | Username | "john_doe" |
| `friendly_name` | User's email or name | "john@example.com" |
| `pin_name` | Name of the PIN used | "John's PIN" |
| `new_state` | New alarm state | "arm", "disarm", "arm_home" |
| `device_name` | Device that made the change | "Kitchen Tablet" |
| `device_ip` | IP address of the device | "192.168.1.100" |
| `event_type` | Always "alarm_state_change" | "alarm_state_change" |

### Example Automations

**Alert if alarm is disarmed at night:**
```yaml
automation:
  - alias: "Alert on night disarm"
    trigger:
      platform: event
      event_type: hass_alarm_state_change
    condition:
      - condition: template
        value_template: "{{ trigger.event.data.new_state == 'disarm' }}"
      - condition: time
        after: '22:00:00'
        before: '06:00:00'
    action:
      - service: notify.mobile_app
        data:
          title: "⚠️ Alarm Disarmed at Night"
          message: >
            {{ trigger.event.data.friendly_name }} disarmed the alarm at
            {{ trigger.event.data.timestamp }}
```

**Log alarm changes:**
```yaml
automation:
  - alias: "Log alarm state changes"
    trigger:
      platform: event
      event_type: hass_alarm_state_change
    action:
      - service: logbook.log
        data:
          name: Alarm Panel
          message: >
            {{ trigger.event.data.friendly_name }} changed alarm to
            {{ trigger.event.data.new_state }} from
            {{ trigger.event.data.device_name }}
```

## Security Features

### PIN Security
- All PINs are hashed using BCrypt (work factor 12)
- PINs are never stored in plain text
- Support for 4-8 digit PINs
- Each user can have their own PIN with a friendly name

### Rate Limiting
- Maximum 5 failed attempts per IP address
- 15-minute lockout after exceeding attempts
- Automatic reset on successful authentication
- Remaining attempts shown in error messages

### Session Management
- Secure 64-byte random tokens
- 30-day expiration (auto-refreshes on activity)
- Each successful PIN entry creates a new session
- Sessions can be revoked via logout endpoint
- Automatic cleanup of expired sessions

### Audit Trail
- All alarm state changes fire events to Home Assistant
- Device names and IP addresses are logged
- Timestamp on every action
- User identification on every change

### IP Filtering (Optional)
- Restrict access to specific IP ranges
- Useful for limiting to local network only
- Returns 403 Forbidden for unauthorized IPs

## Troubleshooting

### Add-on won't start

1. **Check the logs** in the add-on page
2. **Verify configuration** - ensure all required fields are filled
3. **Check input_boolean entities** - make sure they exist in Home Assistant
4. **Verify Home Assistant is accessible** at the configured host

### Cannot log in to admin panel

1. **Check admin credentials** in configuration
2. **Clear browser cache** and cookies
3. **Check add-on logs** for authentication errors
4. **Verify database** was initialized (check logs for migration messages)

### PIN not working

1. **Verify PIN is enabled** in the admin panel
2. **Check user account** is not locked
3. **Verify rate limiting** - wait 15 minutes if locked out
4. **Check PIN length** - must be 4-8 digits
5. **Look for case sensitivity issues** - PINs are numeric only

### Tablet interface not loading

1. **Check network connectivity** from tablet to Home Assistant
2. **Verify port 8099** is accessible
3. **Try clearing browser cache** on tablet
4. **Check firewall rules** if using IP filtering
5. **View browser console** for JavaScript errors

### Events not appearing in Home Assistant

1. **Verify Home Assistant URL** in configuration
2. **Check Supervisor token** is working (automatic)
3. **Look for errors** in add-on logs
4. **Test with Developer Tools** → Events → Listen for `hass_alarm_state_change`
5. **Verify you're changing** the alarm state (events only fire on changes)

### Database errors

1. **Check /data directory** has write permissions
2. **Review migration logs** for errors
3. **If persistent**, stop add-on, backup data, remove /data/hass-alarm.db, restart

### Session expires too quickly

- Sessions last 30 days and refresh on each alarm state change
- If sessions are expiring unexpectedly, check device time synchronization
- Verify the tablet browser isn't clearing local storage

## Advanced Usage

### Multiple Tablets

You can set up unlimited tablets, each with their own device name:

1. Navigate to `/HeadlessPanel` on each tablet
2. Enter a unique device name (e.g., "Kitchen Tablet", "Front Door Panel")
3. Authenticate with any valid PIN
4. Each tablet will have its own 30-day session
5. All device names appear in Home Assistant events

### Custom Integrations

Use the REST API to integrate with:
- Custom mobile apps
- Third-party home automation systems
- Voice assistants
- Smart watches
- IoT devices

See the [Plugin Guide](https://github.com/werddomain/Hass-Alarm/blob/main/PLUGIN_GUIDE.md) for complete API documentation.

### Action Groups (Advanced)

Action Groups allow you to trigger multiple Home Assistant actions when a PIN is entered:

1. Navigate to Admin → Action Groups
2. Create a new action group
3. Add actions (input_boolean toggles or service calls)
4. Assign the action group to a PIN
5. When the PIN is used, all actions in the group execute

## Data Storage

The add-on stores data in `/data/hass-alarm.db` (SQLite):

- User accounts
- PIN codes (hashed)
- Devices
- Sessions
- Action groups and actions

**Backup**: The add-on supports Home Assistant's backup feature. Your data is automatically included in snapshots.

## Performance

- Lightweight: ~50-100MB RAM usage
- Fast: Responses under 100ms
- Efficient: Minimal CPU usage
- Scales: Supports hundreds of users and devices

## Privacy

- No data is sent to external servers
- All data stays on your Home Assistant instance
- No telemetry or analytics
- No external API calls (except to your Home Assistant)

## Support

- **GitHub Issues**: https://github.com/werddomain/Hass-Alarm/issues
- **Documentation**: https://github.com/werddomain/Hass-Alarm
- **Plugin Guide**: https://github.com/werddomain/Hass-Alarm/blob/main/PLUGIN_GUIDE.md

## Version History

See the [Releases](https://github.com/werddomain/Hass-Alarm/releases) page for changelog and version history.
