# Hass-Alarm Installation & Deployment Guide

This guide covers all installation methods for the Hass-Alarm Panel.

## Table of Contents

1. [Home Assistant Add-on Installation](#home-assistant-add-on-installation-recommended)
2. [Docker Installation](#docker-installation)
3. [Manual Installation](#manual-installation)
4. [Production Deployment](#production-deployment)
5. [Upgrading](#upgrading)
6. [Troubleshooting](#troubleshooting)

---

## Home Assistant Add-on Installation (Recommended)

This is the easiest method for Home Assistant users.

### Prerequisites

- Home Assistant OS, Supervised, or Container installation
- Access to the Supervisor panel
- The following entities created in Home Assistant:

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

After adding these, restart Home Assistant.

### Installation Steps

#### Step 1: Add the Repository

1. Navigate to **Supervisor** → **Add-on Store**
2. Click the **⋮** (three dots) menu in the top right corner
3. Select **Repositories**
4. Add the following URL:
   ```
   https://github.com/werddomain/Hass-Alarm
   ```
5. Click **Add**
6. Close the dialog

#### Step 2: Install the Add-on

1. Refresh the Add-on Store page
2. Scroll down to find **Hass-Alarm Panel**
3. Click on it
4. Click **Install**
5. Wait for the installation to complete (this may take a few minutes)

#### Step 3: Configure the Add-on

1. Click on the **Configuration** tab
2. Update the configuration:

```yaml
ha_host: "homeassistant.local:8123"
ha_use_ssl: false
entities:
  arm: "input_boolean.alarm_home"
  arm_home: "input_boolean.alarm_ar"
admin:
  email: "your-email@example.com"
  password: "YourSecurePassword123!"
security:
  allowed_ip_range: ""
  enable_rate_limiting: true
```

**Important Configuration Notes:**
- Change `admin.email` to your email
- **Change `admin.password`** to a strong, unique password
- If using HTTPS for Home Assistant, set `ha_use_ssl: true`
- Adjust `ha_host` if your setup is different
- Optionally set `allowed_ip_range` to restrict access (e.g., `"192.168.1.1-192.168.1.255"`)

3. Click **Save**

#### Step 4: Start the Add-on

1. Click on the **Info** tab
2. Toggle **Start on boot** to ON (recommended)
3. Toggle **Watchdog** to ON (recommended)
4. Click **Start**
5. Wait for the add-on to start (check the Logs tab for progress)

#### Step 5: Verify Installation

1. Click **Open Web UI** or navigate to `http://homeassistant.local:8099`
2. Log in with your admin credentials
3. You should see the Hass-Alarm admin panel

#### Step 6: Initial Setup

1. Navigate to **Users** and create user accounts
2. Navigate to **PIN Codes** and assign PINs to users
3. Test the panel at `/Home/Panel` or `/HeadlessPanel`

---

## Docker Installation

For manual Docker deployments without Home Assistant.

### Prerequisites

- Docker installed
- MySQL 8.0+ database (or use the included docker-compose)
- Home Assistant instance with API access

### Using Docker Compose (Recommended)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/werddomain/Hass-Alarm.git
   cd Hass-Alarm
   ```

2. **Create environment file:**
   ```bash
   cp .env.example .env
   nano .env
   ```

3. **Configure environment variables:**
   ```env
   # Database
   MYSQL_ROOT_PASSWORD=your_root_password
   MYSQL_DATABASE=hass_alarm
   MYSQL_USER=hass_alarm
   MYSQL_PASSWORD=your_db_password

   # Application
   ConnectionStrings__DefaultConnection=Server=hass-alarm-mysql;Database=hass_alarm;User=hass_alarm;Password=your_db_password;
   Ha__Host=your-ha-instance:8123/
   Ha__ApiKey=your_home_assistant_long_lived_token
   Ha__Entities__Arm=input_boolean.alarm_home
   Ha__Entities__ArmHome=input_boolean.alarm_ar
   Admin__Email=admin@example.com
   Admin__Password=YourSecurePassword123!
   Security__AllowedIpRange=
   ```

4. **Start the services:**
   ```bash
   docker-compose up -d
   ```

5. **Check logs:**
   ```bash
   docker-compose logs -f hass-alarm
   ```

6. **Access the application:**
   - Navigate to `http://localhost:5000`

### Using Docker Run

```bash
# Create a network
docker network create hass-alarm-network

# Run MySQL
docker run -d \
  --name hass-alarm-mysql \
  --network hass-alarm-network \
  -e MYSQL_ROOT_PASSWORD=root_password \
  -e MYSQL_DATABASE=hass_alarm \
  -e MYSQL_USER=hass_alarm \
  -e MYSQL_PASSWORD=db_password \
  -v hass-alarm-mysql-data:/var/lib/mysql \
  mysql:8.0.19

# Wait for MySQL to be ready
sleep 30

# Run Hass-Alarm
docker run -d \
  --name hass-alarm \
  --network hass-alarm-network \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Server=hass-alarm-mysql;Database=hass_alarm;User=hass_alarm;Password=db_password;" \
  -e Ha__Host="your-ha-instance:8123/" \
  -e Ha__ApiKey="your_long_lived_token" \
  -e Ha__Entities__Arm="input_boolean.alarm_home" \
  -e Ha__Entities__ArmHome="input_boolean.alarm_ar" \
  -e Admin__Email="admin@example.com" \
  -e Admin__Password="YourSecurePassword123!" \
  ghcr.io/werddomain/hass-alarm:latest
```

---

## Manual Installation

For development or custom deployments.

### Prerequisites

- .NET Core 3.0 SDK
- MySQL 8.0+ or SQLite
- Home Assistant instance with API access

### Installation Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/werddomain/Hass-Alarm.git
   cd Hass-Alarm/Hass-Alarm
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Configure appsettings.json:**
   ```bash
   cp appsettings.json appsettings.Production.json
   nano appsettings.Production.json
   ```

   Update the configuration:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=hass_alarm;User=root;Password=your_password;"
     },
     "Ha": {
       "Host": "your-ha-instance:8123/",
       "ApiKey": "your_long_lived_token",
       "Entities": {
         "Arm": "input_boolean.alarm_home",
         "ArmHome": "input_boolean.alarm_ar"
       }
     },
     "Admin": {
       "Email": "admin@example.com",
       "Password": "YourSecurePassword123!"
     },
     "Security": {
       "AllowedIpRange": ""
     }
   }
   ```

4. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Build the application:**
   ```bash
   dotnet build -c Release
   ```

6. **Run the application:**
   ```bash
   dotnet run --urls "http://0.0.0.0:5000"
   ```

7. **Access the application:**
   - Navigate to `http://localhost:5000`

---

## Production Deployment

### Security Checklist

Before deploying to production, ensure:

- [ ] Changed default admin password
- [ ] Using strong, unique passwords
- [ ] HTTPS/SSL enabled (reverse proxy recommended)
- [ ] Database credentials are secure
- [ ] Rate limiting is enabled
- [ ] IP range filtering configured (if needed)
- [ ] Home Assistant long-lived token is secure
- [ ] Regular backups configured
- [ ] Logs are being monitored

### Reverse Proxy Setup (Nginx)

For HTTPS and additional security:

```nginx
server {
    listen 443 ssl http2;
    server_name alarm.yourdomain.com;

    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
}
```

### Systemd Service (Linux)

For automatic startup on Linux servers:

```ini
# /etc/systemd/system/hass-alarm.service
[Unit]
Description=Hass-Alarm Panel
After=network.target mysql.service

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/hass-alarm
ExecStart=/usr/bin/dotnet /opt/hass-alarm/Hass-Alarm.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=hass-alarm
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable hass-alarm
sudo systemctl start hass-alarm
sudo systemctl status hass-alarm
```

### Database Backup

#### MySQL Backup Script

```bash
#!/bin/bash
# backup-hass-alarm.sh

BACKUP_DIR="/backups/hass-alarm"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="hass_alarm"
DB_USER="hass_alarm"
DB_PASS="your_password"

mkdir -p $BACKUP_DIR

mysqldump -u $DB_USER -p$DB_PASS $DB_NAME | gzip > $BACKUP_DIR/hass_alarm_$DATE.sql.gz

# Keep only last 30 days of backups
find $BACKUP_DIR -name "hass_alarm_*.sql.gz" -mtime +30 -delete

echo "Backup completed: $BACKUP_DIR/hass_alarm_$DATE.sql.gz"
```

Add to crontab for daily backups:
```bash
0 2 * * * /path/to/backup-hass-alarm.sh
```

---

## Upgrading

### Home Assistant Add-on

1. Navigate to **Supervisor** → **Dashboard**
2. Find **Hass-Alarm Panel**
3. If an update is available, click **Update**
4. Wait for the update to complete
5. Check the changelog for breaking changes
6. Restart the add-on if needed

### Docker

```bash
# Pull latest image
docker pull ghcr.io/werddomain/hass-alarm:latest

# Stop and remove old container
docker stop hass-alarm
docker rm hass-alarm

# Run new container (use same command as installation)
docker run -d ...
```

### Manual

```bash
cd Hass-Alarm
git pull origin main
cd Hass-Alarm
dotnet restore
dotnet ef database update
dotnet build -c Release
# Restart the service
```

---

## Version Control & Releases

### Automatic Releases

The project uses GitHub Actions for automatic builds and releases:

- **Automatic builds** on every push to `main`
- **Version bumping** via workflow dispatch
- **Multi-architecture support** (amd64, arm64, armv7, armhf, i386)
- **Semantic versioning**

### Creating a New Release

#### Using GitHub Actions (Recommended)

1. Go to **Actions** tab in GitHub
2. Select **Version Bump** workflow
3. Click **Run workflow**
4. Select version bump type:
   - **patch** (1.0.0 → 1.0.1) - Bug fixes
   - **minor** (1.0.0 → 1.1.0) - New features
   - **major** (1.0.0 → 2.0.0) - Breaking changes
5. Click **Run workflow**

This will:
- Bump the version in `config.json`
- Create a git tag
- Build Docker images for all architectures
- Create a GitHub release
- Update the repository

#### Manual Release

```bash
# Update version in config.json
nano hass-alarm/config.json

# Commit changes
git add hass-alarm/config.json
git commit -m "Bump version to 1.0.1"

# Create tag
git tag -a v1.0.1 -m "Release v1.0.1"

# Push to GitHub
git push origin main
git push origin v1.0.1
```

---

## Troubleshooting

### Common Issues

#### Add-on won't start

**Symptoms:** Add-on shows as stopped, errors in logs

**Solutions:**
1. Check configuration syntax (must be valid YAML)
2. Verify input_boolean entities exist in Home Assistant
3. Check Home Assistant is accessible at configured host
4. Review logs for specific errors
5. Ensure database can be created in `/data` directory

#### Cannot access web interface

**Symptoms:** Connection refused, timeout errors

**Solutions:**
1. Verify add-on is running (check Info tab)
2. Check port 8099 is not in use by another service
3. Try accessing via `http://homeassistant.local:8099`
4. Check firewall rules
5. Review add-on logs for binding errors

#### Database migration errors

**Symptoms:** Errors about tables not existing

**Solutions:**
1. Stop the add-on
2. Delete `/data/hass-alarm.db` (backup first!)
3. Start the add-on (database will be recreated)
4. If persistent, check disk space

#### Events not firing to Home Assistant

**Symptoms:** No events appear in Home Assistant

**Solutions:**
1. Verify Supervisor token is working (automatic in add-on)
2. Check Home Assistant host configuration
3. Test with Developer Tools → Events → Listen for `hass_alarm_state_change`
4. Ensure you're actually changing the alarm state
5. Check add-on logs for errors

#### Session expires immediately

**Symptoms:** Have to re-enter PIN every time

**Solutions:**
1. Check browser isn't blocking cookies/local storage
2. Verify tablet time is synchronized
3. Check browser isn't in private/incognito mode
4. Clear browser cache and try again

### Getting Help

If you can't resolve an issue:

1. Check the [documentation](https://github.com/werddomain/Hass-Alarm)
2. Search [existing issues](https://github.com/werddomain/Hass-Alarm/issues)
3. Create a new issue with:
   - Your configuration (redact passwords!)
   - Relevant logs
   - Steps to reproduce
   - Expected vs actual behavior

---

## Monitoring & Maintenance

### Log Monitoring

#### Home Assistant Add-on
- View logs in **Supervisor** → **Hass-Alarm Panel** → **Log** tab
- Enable watchdog for automatic restarts

#### Docker
```bash
docker logs -f hass-alarm
```

#### Manual
```bash
journalctl -u hass-alarm -f
```

### Health Checks

Monitor these indicators:

1. **Application Status**: Should be running
2. **Database Connection**: Check for connection errors in logs
3. **Home Assistant API**: Verify events are being fired
4. **Session Count**: Monitor active sessions
5. **Failed PIN Attempts**: Watch for unusual activity

### Performance Tuning

For large deployments:

1. **Database**: Use MySQL instead of SQLite
2. **Caching**: Already implemented via MemoryCache
3. **Connection Pooling**: Configured automatically
4. **Session Cleanup**: Runs automatically

---

## Next Steps

After installation:

1. Read the [Plugin Guide](PLUGIN_GUIDE.md) for API usage
2. Set up [Home Assistant automations](hass-alarm/DOCS.md#home-assistant-integration)
3. Configure tablets for headless panel
4. Set up backups
5. Configure HTTPS (if not using add-on)

---

## Additional Resources

- **Documentation**: [hass-alarm/DOCS.md](hass-alarm/DOCS.md)
- **API Guide**: [PLUGIN_GUIDE.md](PLUGIN_GUIDE.md)
- **GitHub Repository**: https://github.com/werddomain/Hass-Alarm
- **Issues**: https://github.com/werddomain/Hass-Alarm/issues
