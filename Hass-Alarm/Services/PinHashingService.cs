using System;

namespace Hass_Alarm.Services
{
    public interface IPinHashingService
    {
        /// <summary>
        /// Hashes a PIN code using BCrypt
        /// </summary>
        string HashPin(string pin);

        /// <summary>
        /// Verifies a PIN against its hash
        /// </summary>
        bool VerifyPin(string pin, string hash);

        /// <summary>
        /// Checks if a string is already hashed (BCrypt format)
        /// </summary>
        bool IsHashed(string value);
    }

    public class PinHashingService : IPinHashingService
    {
        // BCrypt work factor (cost) - higher = more secure but slower
        // 12 is a good balance between security and performance
        private const int WorkFactor = 12;

        public string HashPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                throw new ArgumentException("PIN cannot be null or empty", nameof(pin));
            }

            return BCrypt.Net.BCrypt.HashPassword(pin, WorkFactor);
        }

        public bool VerifyPin(string pin, string hash)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(pin, hash);
            }
            catch
            {
                // If verification fails (invalid hash format), return false
                return false;
            }
        }

        public bool IsHashed(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // BCrypt hashes always start with $2a$, $2b$, or $2y$ and are 60 characters long
            return value.StartsWith("$2") && value.Length == 60;
        }
    }
}
