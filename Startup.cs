using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace VNPTBKN.API {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            //Add service for accessing current HttpContext AND ActionContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                //Cookie
                .Configure<CookiePolicyOptions>(options => {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
            // Authentication JwtBearer
            var Issuer = new System.Collections.Generic.List<string>() {
                "http://localhost:5000",
                "http://10.17.20.99/portal"
            };
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                    // The signing key must match!  
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    // Validate the JWT Issuer (iss) claim  
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    // Validate the JWT Audience (aud) claim  
                    ValidateAudience = true,
                    ValidAudience = Configuration["Jwt:Issuer"],
                    // Validate the token expiry  
                    ValidateLifetime = true,
                    // ClockSkew = TimeSpan.Zero
                    };
                });
            // services.AddDataProtection()
            //     .PersistKeysToFileSystem(GetKeyRingDirInfo())
            //     .SetApplicationName("SharedCookieApp");
            services
                .ConfigureApplicationCookie(options => {
                    options.Cookie.Name = ".AspNet.SharedCookie";
                })
                .AddSession()
                .AddCors()
                .AddMvc()
                .AddSessionStateTempDataProvider()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            //Add service for accessing current HttpContext AND ActionContext
            TM.Core.HttpContext.Services = app.ApplicationServices;
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors(builder =>
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().AllowCredentials()
                // .WithOrigins(
                //     "http://localhost:5000",
                //     "http://10.17.20.99/portal")
            );
            // Authentication JwtBearer
            app.UseAuthentication();

            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}