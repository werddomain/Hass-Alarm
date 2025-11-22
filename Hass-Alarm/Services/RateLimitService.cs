using Microsoft.Extensions.Caching.Memory;
using System;

namespace Hass_Alarm.Services
{
    public interface IRateLimitService
    {
        bool IsBlocked(string identifier);
        void RecordFailedAttempt(string identifier);
        void ResetAttempts(string identifier);
        int GetRemainingAttempts(string identifier);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly IMemoryCache _cache;
        private const int MaxAttempts = 5;
        private const int BlockDurationMinutes = 15;
        private const int AttemptsResetMinutes = 5;

        public RateLimitService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsBlocked(string identifier)
        {
            var blockKey = $"blocked_{identifier}";
            return _cache.TryGetValue(blockKey, out _);
        }

        public void RecordFailedAttempt(string identifier)
        {
            var attemptsKey = $"attempts_{identifier}";
            var blockKey = $"blocked_{identifier}";

            // Get current attempt count
            if (!_cache.TryGetValue(attemptsKey, out int attempts))
            {
                attempts = 0;
            }

            attempts++;

            if (attempts >= MaxAttempts)
            {
                // Block the identifier
                _cache.Set(blockKey, true, TimeSpan.FromMinutes(BlockDurationMinutes));
                _cache.Remove(attemptsKey); // Clear attempts counter
            }
            else
            {
                // Update attempts counter with sliding expiration
                _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(AttemptsResetMinutes));
            }
        }

        public void ResetAttempts(string identifier)
        {
            var attemptsKey = $"attempts_{identifier}";
            _cache.Remove(attemptsKey);
        }

        public int GetRemainingAttempts(string identifier)
        {
            var attemptsKey = $"attempts_{identifier}";

            if (!_cache.TryGetValue(attemptsKey, out int attempts))
            {
                return MaxAttempts;
            }

            return MaxAttempts - attempts;
        }
    }
}
