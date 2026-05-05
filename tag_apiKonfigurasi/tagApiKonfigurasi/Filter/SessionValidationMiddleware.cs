using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using tagApiKonfigurasi.Data;
using tagApiKonfigurasi.Model;

namespace tagApiKonfigurasi.Filter
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
                var tokenVersion = context.User.FindFirst("SessionVersion")?.Value;
                var modul = context.User.FindFirst("modul")?.Value;

                if (!string.IsNullOrEmpty(username) &&
                    !string.IsNullOrEmpty(tokenVersion) &&
                    !string.IsNullOrEmpty(modul))
                {
                    // 🔥 ambil session berdasarkan modul
                    var session = await db.UserSession
                        .FirstOrDefaultAsync(x => x.Username == username && x.Modul == modul);

                    if (session == null || session.SessionVersion.ToString() != tokenVersion)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session expired");
                        return;
                    }
                }
            }

            await _next(context);
        }

        //public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
        //{
        //    if (context.User.Identity?.IsAuthenticated == true)
        //    {
        //        var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
        //        var tokenVersion = context.User.FindFirst("SessionVersion")?.Value;

        //        if (username != null && tokenVersion != null)
        //        {
        //            var user = await userManager.FindByNameAsync(username);

        //            if (user == null || user.SessionVersion.ToString() != tokenVersion)
        //            {
        //                context.Response.StatusCode = 401;
        //                await context.Response.WriteAsync("Session expired");
        //                return;
        //            }
        //        }
        //    }

        //    await _next(context);
        //}
    }
}
