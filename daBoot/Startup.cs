using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using daBoot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using daBoot.Models;
using Newtonsoft.Json;
using System.Text;

namespace daBoot
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
                options.UseMySQL(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddControllersWithViews();
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options => { 
                    options.LoginPath = "/home/login";
                    options.AccessDeniedPath = "/home/accessdenied";
                })
                .AddGitHub("github",options => {
                    options.ClientId = Configuration["GitHub:ClientId"];
                    options.ClientSecret = Configuration["GitHub:ClientSecret"];
                    options.CallbackPath = new PathString("/signin-github");
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize?prompt=consent";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";
                    options.AccessDeniedPath = "/";
                    options.SaveTokens = true;
                    options.Scope.Clear();
                    options.Scope.Add("read:user");
                    options.Scope.Add("user:email");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();
                            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                            var id = context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                            var username = "(GH)" + context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
                            var name = context.Identity.Claims.FirstOrDefault(c => c.Type == "urn:github:name").Value;
                            var url = context.Identity.Claims.FirstOrDefault(c => c.Type == "urn:github:url").Value;
                            var email = context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value;
                            var avatar = json.RootElement.GetProperty("avatar_url").ToString();
                            var nameparts = name.Split(' ', 2);
                            if (nameparts.Length == 2) {
                                name = nameparts[0] + " " + nameparts[1].Replace(" ", "");
                            } else
                            {
                                name = nameparts[0];
                            }
                            context.Identity.RemoveClaim(context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));
                            context.Identity.RemoveClaim(context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name));
                            context.Identity.RemoveClaim(context.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email));
                            context.Identity.RemoveClaim(context.Identity.Claims.FirstOrDefault(c => c.Type == "urn:github:name"));
                            context.Identity.RemoveClaim(context.Identity.Claims.FirstOrDefault(c => c.Type == "urn:github:url"));
                            context.Identity.AddClaim(new Claim("username", username));
                            context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));
                            context.Identity.AddClaim(new Claim(ClaimTypes.Name, name));
                            context.Identity.AddClaim(new Claim(ClaimTypes.Email, email));
                            context.Identity.AddClaim(new Claim("profpic", avatar));
                        }
                    };
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Use wwwroot files

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                /*endpoints.MapControllerRoute(
                    name: "profile",
                    pattern: "profile/{username?}",
                    defaults: new { controller = "Profile", action = "Index" }); */
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
