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
using System.IO;
using Renci.SshNet;
using MySql.Data.MySqlClient;

namespace daBoot
{
    public class Startup
    {
        public static (SshClient SshClient, uint Port) ConnectSsh(string sshHostName, string sshUserName, string sshPassword = null,
            string sshKeyFile = null, string sshPassPhrase = null, int sshPort = 22, string databaseServer = "localhost", int databasePort = 3306)
        {
            // check arguments
            if (string.IsNullOrEmpty(sshHostName))
                throw new ArgumentException($"{nameof(sshHostName)} must be specified.", nameof(sshHostName));
            if (string.IsNullOrEmpty(sshHostName))
                throw new ArgumentException($"{nameof(sshUserName)} must be specified.", nameof(sshUserName));
            if (string.IsNullOrEmpty(sshPassword) && string.IsNullOrEmpty(sshKeyFile))
                throw new ArgumentException($"One of {nameof(sshPassword)} and {nameof(sshKeyFile)} must be specified.");
            if (string.IsNullOrEmpty(databaseServer))
                throw new ArgumentException($"{nameof(databaseServer)} must be specified.", nameof(databaseServer));

            // define the authentication methods to use (in order)
            var authenticationMethods = new List<AuthenticationMethod>();
            if (!string.IsNullOrEmpty(sshKeyFile))
            {
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(sshUserName,
                    new PrivateKeyFile(sshKeyFile, string.IsNullOrEmpty(sshPassPhrase) ? null : sshPassPhrase)));
            }
            if (!string.IsNullOrEmpty(sshPassword))
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(sshUserName, sshPassword));
            }

            // connect to the SSH server
            var sshClient = new SshClient(new Renci.SshNet.ConnectionInfo(sshHostName, sshPort, sshUserName, authenticationMethods.ToArray()));
            sshClient.Connect();

            // forward a local port to the database server and port, using the SSH server
            var forwardedPort = new ForwardedPortLocal("127.0.0.1", databaseServer, (uint)databasePort);
            sshClient.AddForwardedPort(forwardedPort);
            forwardedPort.Start();

            return (sshClient, forwardedPort.BoundPort);
        }


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
            
            /*
            services.AddDbContext<ApplicationDbContext>(options =>
            {

                var myConnectionSettings = Configuration.GetSection("ConnectionSettings").Get<ConnectionSettings>();
                var sshSettings = myConnectionSettings.SSH;
                var databaseSettings = myConnectionSettings.Database;
                string keyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "test4");
                var authMethods = new List<AuthenticationMethod>
                {
                    new PrivateKeyAuthenticationMethod(sshSettings.UserName, new PrivateKeyFile(keyFilePath))
                };
                using (var sshClient = new SshClient(new Renci.SshNet.ConnectionInfo(sshSettings.Server,
                    sshSettings.Port, sshSettings.UserName, authMethods.ToArray())))
                {
                    sshClient.Connect();
                    var forwardedPort = new ForwardedPortLocal(databaseSettings.BoundHost, databaseSettings.Host,
                        databaseSettings.Port);
                    sshClient.AddForwardedPort(forwardedPort);
                    forwardedPort.Start();


                    MySqlConnectionStringBuilder csb;
                    csb = new MySqlConnectionStringBuilder
                    {
                        Server = databaseSettings.BoundHost,
                        Port = forwardedPort.BoundPort,
                        UserID = databaseSettings.UserName,
                        Password = databaseSettings.Password,
                        Database = databaseSettings.DatabaseName
                    };
                    options.UseMySQL(csb.ConnectionString);
                }

            }); 
            */

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