using Hass_Alarm.Filters.IpFilter;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseIPFilter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware>();
        }
    }
}
