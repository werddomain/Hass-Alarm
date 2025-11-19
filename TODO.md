# Hass-Alarm - Technical Debt & Future Improvements

This document tracks minor issues, technical debt, and future enhancements identified during code reviews.

---

## 🔴 CRITICAL - Security Issues (High Priority)

### 1. **PIN Codes Stored in Plain Text**
- **Priority**: CRITICAL - Requires planning
- **Impact**: Major security vulnerability if database is compromised
- **Current State**: PIN codes are stored as plain text in the database
- **Recommendation**:
  - Implement PIN hashing using a secure algorithm (e.g., BCrypt, Argon2)
  - Create database migration to hash existing PINs
  - Update all PIN validation logic to compare hashes
  - **NOTE**: This is a breaking change requiring careful migration planning
  - **Estimated Effort**: 4-8 hours
- **Files Affected**:
  - `Data/Models/PinCodes.cs`
  - `Controllers/HomeController.cs`
  - `Controllers/MyPinController.cs`
  - `Areas/Admin/Controllers/UsersController.cs`
  - Database migration required

### 2. **No Rate Limiting on PIN Attempts**
- **Priority**: HIGH
- **Impact**: Vulnerable to brute force attacks
- **Recommendation**:
  - Implement rate limiting middleware for PIN validation endpoint
  - Add account lockout after X failed attempts
  - Consider implementing CAPTCHA after multiple failures
  - Log all failed attempts for security monitoring
- **Estimated Effort**: 2-4 hours
- **Files Affected**:
  - `Controllers/HomeController.cs:Panel()` method
  - New middleware class

---

## 🟡 MEDIUM - Performance & Code Quality

### 3. **Unused Variables in HomeController**
- **Priority**: LOW
- **Impact**: Code cleanliness
- **Details**:
  - `Entity_Arm` and `Entity_ArmHome` are declared but never used (lines 24-25)
- **Action**: Remove unused variables
- **Estimated Effort**: 5 minutes
- **File**: `Controllers/HomeController.cs`

### 4. **Non-Compiled Regex Pattern**
- **Priority**: LOW
- **Impact**: Minor performance improvement
- **Current Code**:
  ```csharp
  System.Text.RegularExpressions.Regex.IsMatch(pinCode.Pin, @"^\d{4,8}$")
  ```
- **Recommendation**: Create static compiled Regex for better performance
  ```csharp
  private static readonly Regex PinRegex = new Regex(@"^\d{4,8}$", RegexOptions.Compiled);
  ```
- **Estimated Effort**: 15 minutes
- **Files Affected**:
  - `Areas/Admin/Controllers/UsersController.cs`
  - `Controllers/MyPinController.cs`

### 5. **Missing Validation Attributes on PinCode Model**
- **Priority**: LOW
- **Impact**: Data integrity
- **Recommendation**: Add data annotations to PinCode model
  ```csharp
  [Required]
  [StringLength(100, MinimumLength = 3)]
  public string Name { get; set; }

  [Required]
  [RegularExpression(@"^\d{4,8}$", ErrorMessage = "PIN must be 4-8 digits")]
  public string Pin { get; set; }
  ```
- **Estimated Effort**: 30 minutes
- **File**: `Data/Models/PinCodes.cs`

---

## 🟢 LOW - Nice to Have

### 6. **N+1 Query Issue with Role Checking**
- **Priority**: MEDIUM
- **Impact**: Performance degradation with many users
- **Current Issue**: `IsInRoleAsync` is called 3 times per user in a loop
- **Location**: `Areas/Admin/Controllers/UsersController.cs:68-70`
- **Recommendation**: Batch load user roles
  ```csharp
  // Load all user roles at once
  var userRoles = new Dictionary<string, IList<string>>();
  foreach (var user in users)
  {
      userRoles[user.Id] = await _userManager.GetRolesAsync(user);
  }
  ```
- **Estimated Effort**: 1-2 hours
- **File**: `Areas/Admin/Controllers/UsersController.cs`

### 7. **No Audit Trail for Alarm Actions**
- **Priority**: MEDIUM
- **Impact**: Security and compliance
- **Recommendation**:
  - Create an `AuditLog` table to track all alarm state changes
  - Log: User, PIN used, Action, Timestamp, Result
  - Add admin interface to view audit logs
- **Estimated Effort**: 4-6 hours
- **New Files**:
  - `Data/Models/AuditLog.cs`
  - `Areas/Admin/Controllers/AuditLogsController.cs`
  - Views and migration

### 8. **No Unit Tests**
- **Priority**: MEDIUM
- **Impact**: Code maintainability and reliability
- **Recommendation**:
  - Add unit test project
  - Test critical paths: PIN validation, alarm state changes, user management
  - Aim for 70%+ code coverage
- **Estimated Effort**: 8-16 hours

### 9. **Missing Action Execution in Panel**
- **Priority**: MEDIUM - Partially completed
- **Status**: ✅ COMPLETED - Implemented in latest code review
- **Details**: Alarm actions (arm/disarm/arm_home) are now properly implemented

### 10. **Improve Error Messages for Users**
- **Priority**: LOW
- **Impact**: User experience
- **Recommendation**:
  - Create a unified error message system
  - Add user-friendly error pages
  - Localization support for multi-language
- **Estimated Effort**: 2-4 hours

---

## 📋 Future Features

### 11. **Multi-Factor Authentication (MFA)**
- Add support for TOTP (Time-based One-Time Password)
- SMS verification option
- Backup codes

### 12. **Geofencing**
- Auto-arm/disarm based on user location
- Requires mobile app integration

### 13. **Scheduled Arming/Disarming**
- Allow users to set schedules for automatic arming
- Integration with calendar

### 14. **PIN Expiration Policy**
- Force PIN rotation after X days
- Admin-configurable expiration settings

### 15. **Notification System**
- Email/SMS notifications for alarm events
- Push notifications via mobile app
- Webhook support for integrations

---

## 📊 Technical Debt Summary

| Priority | Count | Estimated Total Effort |
|----------|-------|------------------------|
| Critical | 2     | 6-12 hours            |
| High     | 0     | 0 hours               |
| Medium   | 4     | 15-28 hours           |
| Low      | 4     | 1.5 hours             |

**Total Estimated Effort**: 22.5 - 41.5 hours

---

## ✅ Recently Completed Issues

### Code Review #1 (Completed)
- ✅ Fixed N+1 query problem in UsersController.Index()
- ✅ Replaced .Result anti-pattern with async/await
- ✅ Fixed NullReferenceException vulnerabilities
- ✅ Added structured logging throughout
- ✅ Improved PIN validation with duplicate detection
- ✅ Enabled LockoutEnabled when locking users

### Code Review #2 (Completed)
- ✅ Fixed missing Enabled check on PIN validation
- ✅ Converted synchronous queries to async in HomeController
- ✅ Implemented alarm actions (arm/disarm/arm_home)
- ✅ Fixed synchronous queries in MyPinController
- ✅ Added duplicate PIN validation in MyPinController
- ✅ Added parameter validation throughout UsersController
- ✅ Added TempData for user feedback messages
- ✅ Added comprehensive logging to MyPinController

---

## 📝 Notes

- This document should be updated after each code review
- Prioritization may change based on business needs
- Effort estimates are rough and may vary
- Security issues should always be addressed first

Last Updated: 2025-11-19
