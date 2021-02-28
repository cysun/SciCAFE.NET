using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SciCAFE.NET.Models;
using SciCAFE.NET.Security;
using SciCAFE.NET.Security.Constants;
using SciCAFE.NET.Services;
using Serilog;

namespace SciCAFE.NET
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = true)
                  .AddEntityFrameworkStores<AppDbContext>()
                  .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.IsAdministrator, policyBuilder =>
                    policyBuilder.RequireClaim(ClaimType.IsAdministrator, "true"));
                options.AddPolicy(Policy.IsEventReviewer, policyBuilder =>
                    policyBuilder.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == ClaimType.IsAdministrator || c.Type == ClaimType.IsEventReviewer) && c.Value == "true")));
                options.AddPolicy(Policy.IsRewardReviewer, policyBuilder =>
                    policyBuilder.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            (c.Type == ClaimType.IsAdministrator || c.Type == ClaimType.IsRewardReviewer) && c.Value == "true")));
                options.AddPolicy(Policy.CanEditEvent, policyBuilder =>
                    policyBuilder.AddRequirements(new CanEditEventRequirement()));
                options.AddPolicy(Policy.CanDeleteEvent, policyBuilder =>
                    policyBuilder.AddRequirements(new CanDeleteEventRequirement()));
                options.AddPolicy(Policy.CanReviewEvent, policyBuilder =>
                    policyBuilder.AddRequirements(new CanReviewEventRequirement()));
                options.AddPolicy(Policy.CanManageAttendance, policyBuilder =>
                    policyBuilder.AddRequirements(new CanManageAttendanceRequirement()));
                options.AddPolicy(Policy.CanEditReward, policyBuilder =>
                    policyBuilder.AddRequirements(new CanEditRewardRequirement()));
                options.AddPolicy(Policy.CanDeleteReward, policyBuilder =>
                    policyBuilder.AddRequirements(new CanDeleteRewardRequirement()));
                options.AddPolicy(Policy.CanReviewReward, policyBuilder =>
                    policyBuilder.AddRequirements(new CanReviewRewardRequirement()));
                options.AddPolicy(Policy.CanAddQualifyingEvent, policyBuilder =>
                    policyBuilder.AddRequirements(new CanAddQualifyingEventRequirement()));
                options.AddPolicy(Policy.CanViewRewardees, policyBuilder =>
                    policyBuilder.AddRequirements(new CanViewRewardeesRequirement()));
                options.AddPolicy(Policy.CanEmail, policyBuilder =>
                    policyBuilder.AddRequirements(new CanEmailRequirement()));
            });
            services.AddScoped<IAuthorizationHandler, CanEditEventHandler>();
            services.AddScoped<IAuthorizationHandler, CanDeleteEventHandler>();
            services.AddScoped<IAuthorizationHandler, CanReviewEventHandler>();
            services.AddScoped<IAuthorizationHandler, CanManageAttendanceHandler>();
            services.AddScoped<IAuthorizationHandler, CanEditRewardHandler>();
            services.AddScoped<IAuthorizationHandler, CanDeleteRewardHandler>();
            services.AddScoped<IAuthorizationHandler, CanReviewRewardHandler>();
            services.AddScoped<IAuthorizationHandler, CanAddQualifyingEventHandler>();
            services.AddScoped<IAuthorizationHandler, CanViewRewardeesHandler>();
            services.AddScoped<IAuthorizationHandler, CanEmailAttendeesHandler>();
            services.AddScoped<IAuthorizationHandler, CanEmailRewardeesHandler>();

            services.AddAutoMapper(config => config.AddProfile<MapperProfile>());

            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.AddScoped<EmailSender>();

            services.AddScoped<UserService>();
            services.AddScoped<ProgramService>();
            services.AddScoped<EventService>();
            services.AddScoped<RewardService>();

            services.Configure<FileSettings>(Configuration.GetSection("File"));
            services.AddScoped<FileService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // The default locale on Ubuntu Server is C.UTF-8. Changing it to en_US.UTF-8 presumably
            // will have the same effect, but setting it in application has the advantage of consistence
            // across different servers.
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseSerilogRequestLogging();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UsePathBase(Configuration["Application:PathBase"]);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "attachment",
                    pattern: "Attachment/{type}/{action}/{id}",
                    defaults: new { controller = "Attachment" }
                );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
