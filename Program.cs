﻿
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using WebApplicationFlowSync.Data;
using WebApplicationFlowSync.Errors;
using WebApplicationFlowSync.Models;
using WebApplicationFlowSync.services.EmailService;
using WebApplicationFlowSync.services;
using Task = System.Threading.Tasks.Task;
using WebApplicationFlowSync.services.ExternalServices;
using Microsoft.Graph.Models.ExternalConnectors;
using WebApplicationFlowSync.Classes;
using WebApplicationFlowSync.services.SettingService;
using WebApplicationFlowSync.services.CacheServices;

namespace WebApplicationFlowSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // ????? ??? Identity ?? ???????

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {

                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;


                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                //options.User.RequireUniqueEmail = true;       
            }).AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();





            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Add Cors
            builder.Services.AddCors(
               options =>
               {
                   options.AddPolicy("AllowAll",
                   builder =>
                   {
                       builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();


                   });

               });


            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.Configure<ApplicationSettings>(builder.Configuration);
            builder.Services.AddScoped<ISettingsService,SettingsService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<ICacheService, CacheService>();

            // Email Service 
            RegisterMailServices(builder);


            //Add Auth Service (generate token)
            builder.Services.AddScoped<AuthServices>();


            // Serilog
            builder.Host.UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);
            });


            // JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("jwt")["ValidIssuer"],
            ValidAudience = builder.Configuration.GetSection("jwt")["ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("jwt")["secretKey"])),

            // ✅ هذا مهم جدًا للسماح باستخدام [Authorize(Roles = "...")]
            RoleClaimType = ClaimTypes.Role
        };
    });

            builder.Services.AddAuthorization();

            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ✅ إضافة هذا البلوك لإنشاء الأدوار عند بداية التشغيل
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                Task.Run(async () =>
                {
                    string[] roles = { "Leader", "Member" };
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }
                }).GetAwaiter().GetResult();
            }

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthentication();  // لازم قبل UseAuthorization
            app.UseAuthorization();


            app.MapControllers();

            app.UseExceptionHandler();
            app.Run();
        }

        /// <summary>
        /// RegisterMailServices
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterMailServices(WebApplicationBuilder builder)
        {
            var config = builder.Configuration;
            var microsoftAuthorizationUrl = config["MicrosoftAuthorizationServiceSettings:BaseUrl"] ?? "";
            var emailHost = config.GetSection("EmailSettings")["EmailHost"];
            
            //Register microsoft authorization and graph servcies
            builder.Services.AddScoped<GraphAuthProvider>();
            builder.Services.AddHttpClient<IMicrosoftAuthorizationClient, MicrosoftAuthorizationClient>(client =>
            {
                client.BaseAddress = new Uri(microsoftAuthorizationUrl);
            });

            //Register mail services
            if (emailHost != null && emailHost == "smtp.office365.com")
            {
                builder.Services.AddScoped<IEmailService, OutlookEmailService>();
            }
            else
            {
                builder.Services.AddScoped<IEmailService,EmailService>();
            }
        }
        }
    
}
