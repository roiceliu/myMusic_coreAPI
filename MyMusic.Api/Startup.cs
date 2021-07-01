using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MyMusic.Core;
using MyMusic.Core.Models.Auth;
using MyMusic.Core.Services;
using MyMusic.Data;
using MyMusic.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyMusic.Api.Settings;
using MyMusic.Api.Extensions;
using System.Collections.Generic;

namespace MyMusic.Api
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
            var jwtSettings = Configuration.GetSection("Jwt").Get<JwtSettings>();

            //add dependency injection of unitofwork -scope: assign same object when requested
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddControllers();

             services.AddDbContext<MyMusicDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("Default"), x => x.MigrationsAssembly("MyMusic.Data")));

            //inject dependency: new obj every request
            services.AddTransient<IMusicService, MusicService>();
            services.AddTransient<IArtistService, ArtistService>();

            services.AddSwaggerGen(options =>
            {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My Music",
            Version = "v1" });

            options.AddSecurityDefinition("Bearer", 
            new OpenApiSecurityScheme
            {
                Description = "JWT containing userid claim",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            }
            );

                var security = new OpenApiSecurityRequirement
                {{
                    new OpenApiSecurityScheme{
                        Reference = new OpenApiReference{
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        },
                        UnresolvedReference = true
                    },
                    new List<string>()
                }};

                options.AddSecurityRequirement(security);
            });

            services.AddAutoMapper(typeof(Startup));

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<MyMusicDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<JwtSettings>(Configuration.GetSection("Jwt"));
            services.AddAuth(jwtSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else{
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthorization();
            app.UseAuth();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
            c.RoutePrefix = "";
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Music V1");
            });
            

        }
    }
}
