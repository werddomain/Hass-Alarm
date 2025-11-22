# Hass-Alarm Plugin Guide

This guide explains how to use the Hass-Alarm system as a plugin with headless tablets/screens and through the REST API.

## Features

- **Plugin-Compatible Architecture**: Access alarm panel functionality through REST API or headless web interface
- **Session-Based Authentication**: Secure 30-day sessions that reset on each successful PIN entry
- **Device/Tablet Management**: Track and manage multiple devices accessing the alarm panel
- **Home Assistant Event Integration**: Automatic event firing to Home Assistant on alarm state changes
- **Headless Panel Interface**: Clean, minimal UI optimized for wall-mounted tablets

## Table of Contents

1. [Headless Panel for Tablets](#headless-panel-for-tablets)
2. [REST API Documentation](#rest-api-documentation)
3. [Home Assistant Event Integration](#home-assistant-event-integration)
4. [Security Features](#security-features)

---

## Headless Panel for Tablets

The headless panel provides a clean, fullscreen interface optimized for tablets and wall-mounted screens.

### Accessing the Headless Panel

Navigate to: `http://your-server/HeadlessPanel`

### Features

- **Auto-Authentication**: Sessions are stored locally and persist for 30 days
- **Device Registration**: Each tablet can have a unique name for tracking
- **Real-time State Updates**: Alarm state updates every 5 seconds
- **Touch-Optimized Interface**: Large buttons and clear visual feedback
- **Session Reset**: Each successful alarm action refreshes the 30-day session

### Usage Flow

1. **First Visit**:
   - Enter a device/tablet name (e.g., "Kitchen Tablet", "Entry Wall Panel")
   - Enter your PIN code using the on-screen keypad
   - Press the checkmark to authenticate

2. **After Authentication**:
   - The panel unlocks and shows three action buttons
   - Current alarm state is displayed at the top with color-coded icon
   - Click any action button to change the alarm state
   - Session automatically refreshes with each action

3. **Session Persistence**:
   - Your session is stored locally and lasts 30 days
   - Each time you change the alarm state, the 30-day timer resets
   - If you don't use the panel for 30 days, you'll need to re-authenticate

### Visual States

- **Disarmed**: Green shield icon with "DISARMED" text
- **Armed**: Red shield icon with "ARMED" text
- **Armed Home**: Orange shield icon with "ARMED HOME" text

---

## REST API Documentation

The plugin exposes a REST API for integration with custom applications, scripts, or third-party systems.

### Base URL

```
http://your-server/api/plugin
```

### Authentication Flow

All API requests (except device registration and authentication) require a session token passed in the `X-Session-Token` header.

#### 1. Register Device

Register a new device to get a device ID.

**Endpoint**: `POST /api/plugin/device/register`

**Request Body**:
```json
{
  "deviceName": "Kitchen Tablet",
  "description": "Wall-mounted tablet in kitchen",
  "uniqueIdentifier": "optional-unique-id"
}
```

**Response**:
```json
{
  "device_id": 1,
  "unique_identifier": "abc-123-def",
  "message": "Device registered successfully"
}
```

#### 2. Authenticate

Authenticate with a PIN to receive a session token.

**Endpoint**: `POST /api/plugin/authenticate`

**Request Body**:
```json
{
  "pin": "1234",
  "deviceId": 1
}
```

**Response** (Success):
```json
{
  "session_token": "base64-encoded-token",
  "expires_at": "2025-12-22T00:00:00Z",
  "user_name": "john_doe",
  "device_name": "Kitchen Tablet",
  "message": "Authentication successful"
}
```

**Response** (Failure):
```json
{
  "error": "Invalid PIN",
  "code_invalid": true,
  "remaining_attempts": 4
}
```

**Response** (Rate Limited):
```json
{
  "error": "Too many failed attempts",
  "rate_limited": true,
  "remaining_attempts": 0
}
```

#### 3. Get Alarm State

Retrieve the current alarm state.

**Endpoint**: `GET /api/plugin/state`

**Headers**:
```
X-Session-Token: your-session-token
```

**Response**:
```json
{
  "arm_state": 0,
  "state_name": "disarm",
  "timestamp": "2025-11-22T12:00:00Z"
}
```

**Arm State Values**:
- `0` = Disarm
- `1` = Arm
- `2` = Arm_Home

#### 4. Set Alarm State

Change the alarm state.

**Endpoint**: `POST /api/plugin/state`

**Headers**:
```
X-Session-Token: your-session-token
```

**Request Body**:
```json
{
  "action": "arm"
}
```

**Valid Actions**:
- `disarm`
- `arm`
- `arm_home`

**Response**:
```json
{
  "previous_state": "disarm",
  "new_state": "arm",
  "timestamp": "2025-11-22T12:00:00Z",
  "session_refreshed": true
}
```

**Note**: Each successful state change resets the session expiration to 30 days from now.

#### 5. Logout

Revoke the current session.

**Endpoint**: `POST /api/plugin/logout`

**Headers**:
```
X-Session-Token: your-session-token
```

**Response**:
```json
{
  "message": "Logged out successfully"
}
```

---

## Home Assistant Event Integration

When a PIN is entered and the alarm state changes, the system automatically fires an event to Home Assistant that can be used for notifications and automations.

### Event Details

**Event Type**: `hass_alarm_state_change`

**Event Data**:
```yaml
timestamp: "2025-11-22T12:00:00.000Z"
user_name: "john_doe"
friendly_name: "john@example.com"
pin_name: "John's PIN"
new_state: "arm"
device_name: "Kitchen Tablet"
device_ip: "192.168.1.100"
event_type: "alarm_state_change"
```

### Setting Up Automations

Create automations in Home Assistant to respond to alarm state changes:

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
            at {{ trigger.event.data.timestamp }}
```

### Example Use Cases

1. **Notification on Arming**:
   - Send push notification when alarm is armed
   - Include who armed it and from which device

2. **Disarm Alerts**:
   - Alert if alarm is disarmed at unusual times
   - Track which family member disarmed

3. **Device Tracking**:
   - Know which tablet/device was used
   - Audit trail for security purposes

4. **Time-Based Rules**:
   - Alert if alarm is armed during the day
   - Remind if not armed at night

---

## Security Features

### Rate Limiting

- **Maximum Attempts**: 5 failed PIN attempts per IP address
- **Lockout Duration**: 15 minutes after exceeding attempts
- **Auto-Reset**: Successful authentication resets the counter
- **Response**: API returns `remaining_attempts` in error responses

### Session Management

- **Token Generation**: Cryptographically secure random tokens (64 bytes)
- **Expiration**: 30 days from creation or last activity
- **Auto-Refresh**: Each alarm state change resets the expiration
- **Revocation**: Logout endpoint immediately invalidates session
- **Auto-Cleanup**: Expired sessions are removed from database

### PIN Security

- **Hashing**: All PINs are hashed using BCrypt (work factor 12)
- **Length**: 4-8 digits
- **Storage**: Never stored in plain text
- **Verification**: Constant-time comparison to prevent timing attacks

### Device Security

- **Registration**: Each device must register before use
- **Tracking**: All devices are tracked with unique identifiers
- **Audit Trail**: Last access time recorded for each device
- **Deactivation**: Devices can be disabled without deletion

---

## Example Integration Scripts

### Python Example

```python
import requests
import json

BASE_URL = "http://your-server/api/plugin"

# Register device
register_response = requests.post(f"{BASE_URL}/device/register", json={
    "deviceName": "Python Script",
    "description": "Automation script"
})
device_id = register_response.json()["device_id"]

# Authenticate
auth_response = requests.post(f"{BASE_URL}/authenticate", json={
    "pin": "1234",
    "deviceId": device_id
})
session_token = auth_response.json()["session_token"]

# Get current state
state_response = requests.get(
    f"{BASE_URL}/state",
    headers={"X-Session-Token": session_token}
)
print(f"Current state: {state_response.json()['state_name']}")

# Set state to armed
set_response = requests.post(
    f"{BASE_URL}/state",
    headers={"X-Session-Token": session_token},
    json={"action": "arm"}
)
print(f"New state: {set_response.json()['new_state']}")
```

### cURL Examples

```bash
# Register device
curl -X POST http://your-server/api/plugin/device/register \
  -H "Content-Type: application/json" \
  -d '{"deviceName":"My Device","description":"Test device"}'

# Authenticate
curl -X POST http://your-server/api/plugin/authenticate \
  -H "Content-Type: application/json" \
  -d '{"pin":"1234","deviceId":1}'

# Get state
curl -X GET http://your-server/api/plugin/state \
  -H "X-Session-Token: your-session-token"

# Set state
curl -X POST http://your-server/api/plugin/state \
  -H "Content-Type: application/json" \
  -H "X-Session-Token: your-session-token" \
  -d '{"action":"arm"}'
```

---

## Troubleshooting

### Common Issues

1. **"Invalid or expired session"**
   - Session may have expired (30 days)
   - Re-authenticate to get a new session token

2. **"Too many failed attempts"**
   - Wait 15 minutes before trying again
   - Check that you're using the correct PIN

3. **"Invalid device"**
   - Ensure the device is registered and active
   - Check the device_id is correct

4. **Events not appearing in Home Assistant**
   - Verify Home Assistant API key is correct
   - Check Home Assistant host configuration
   - Look for errors in application logs

### Database Migrations

After deployment, ensure migrations are applied:

```bash
dotnet ef database update
```

Or if using Docker, migrations are applied automatically on startup.

---

## Configuration

### Required Settings (appsettings.json)

```json
{
  "Ha": {
    "Host": "your-ha-instance:8123/",
    "ApiKey": "your-long-lived-access-token",
    "Entities": {
      "Arm": "input_boolean.alarm_home",
      "ArmHome": "input_boolean.alarm_ar"
    }
  }
}
```

### Database

The plugin requires the following database tables (created automatically via migrations):

- `Devices` - Registered tablets/devices
- `PanelSessions` - Active authentication sessions

---

## Best Practices

1. **Session Management**:
   - Store session tokens securely
   - Don't share tokens between devices
   - Logout when device is no longer in use

2. **Device Names**:
   - Use descriptive names (e.g., "Kitchen Wall Tablet")
   - Include location for easier identification

3. **PIN Security**:
   - Use unique PINs for each user
   - Regularly rotate PINs
   - Disable unused PINs in admin panel

4. **Monitoring**:
   - Set up Home Assistant automations to track usage
   - Review device access logs regularly
   - Monitor for suspicious activity patterns

---

## Support

For issues or questions:
1. Check application logs for error details
2. Verify Home Assistant connectivity
3. Review rate limiting status
4. Check database migrations are applied

