using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ClaimsSample.Data;

namespace ClaimsSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseInMemoryDatabase("InMemoryDb"));

            #region snippet_AddGoogle
            services.AddAuthentication().AddGoogle(options =>
            {
                // Provide the Google Client ID
                options.ClientId = "XXXXXXXXXXXXXXX.apps.googleusercontent.com";
                // Register with User Secrets using:
                // dotnet user-secrets set "Authentication:Google:ClientId" "{Client ID}"

                // Provide the Google Client Secret
                options.ClientSecret = "{Client Secret}";
                // Register with User Secrets using:
                // dotnet user-secrets set "Authentication:Google:ClientSecret" "{Client Secret}"

                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
                options.SaveTokens = true;

                options.Events.OnCreatingTicket = ctx =>
                {
                    List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList(); 

                    tokens.Add(new AuthenticationToken()
                    {
                        Name = "TicketCreated", 
                        Value = DateTime.UtcNow.ToString()
                    });

                    ctx.Properties.StoreTokens(tokens);

                    return Task.CompletedTask;
                };
            });
            #endregion

            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                    options.Conventions.AuthorizePage("/MyClaims");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
