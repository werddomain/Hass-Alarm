using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Hass_Alarm.Filters.IpFilter
{

    public class Middleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public Middleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }
        private static uint? start;
        private static uint? end;
        private static uint ConvertFromIpAddressToInteger(IPAddress address)
        {
           
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }
        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
            if (start == null && end == null)
            {
                var configString = _configuration.GetValue<string>("Security:AllowedIpRange");
                if (!String.IsNullOrEmpty(configString))
                {
                    var range = configString.Split("-", StringSplitOptions.RemoveEmptyEntries);
                    start = ConvertFromIpAddressToInteger(IPAddress.Parse(range[0]));
                    end = ConvertFromIpAddressToInteger(IPAddress.Parse(range[1]));
                }
            }
            if (start.HasValue && end.HasValue)
            {
                var currentIpInt = ConvertFromIpAddressToInteger(ipAddress);

                
                if (!(currentIpInt >= start && currentIpInt <= end))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
