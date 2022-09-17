namespace RoleBaseAuth.Server
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using IdentityModel;
    using Finbuckle.MultiTenant;
    using Finbuckle.MultiTenant.Strategies;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using RoleBaseAuth.Server.Data;
    using RoleBaseAuth.Server.Models;
    using RoleBaseAuth.Shared;
    using System.IO;

    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AdditionalUserClaimsPrincipalFactory>();
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
               .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddClaimsPrincipalFactory<AdditionalUserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options =>
                {
                    const string OpenId = "openid";

                    options.IdentityResources[OpenId].UserClaims.Add(CustomClaimTypes.Planet);
                    options.ApiResources.Single().UserClaims.Add(CustomClaimTypes.Planet);

                    options.IdentityResources[OpenId].UserClaims.Add(JwtClaimTypes.Email);
                    options.ApiResources.Single().UserClaims.Add(JwtClaimTypes.Email);

                    options.IdentityResources[OpenId].UserClaims.Add(JwtClaimTypes.Role);
                    options.ApiResources.Single().UserClaims.Add(JwtClaimTypes.Role);
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove(JwtClaimTypes.Role);
            services.AddAuthentication().AddIdentityServerJwt();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddAuthorization(options =>
            {
                options.AddMarsPolicy();
            });

            services.AddMultiTenant<TenantInfo>()
                    .WithConfigurationStore()
                    .WithBasePathStrategy()
                    .WithPerTenantAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
               IApplicationBuilder app,
               IWebHostEnvironment env,
               RoleManager<IdentityRole> roleManager,
               UserManager<ApplicationUser> userManager)
        {

            ApplicationDbInitialiser.SeedRoles(roleManager);
            ApplicationDbInitialiser.SeedUsers(userManager);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMultiTenant();
            app.Use(async (context, next) =>
            {
                var mtc = context.GetMultiTenantContext<TenantInfo>();
                var tenant = mtc?.TenantInfo;
                if (tenant != null && mtc.StrategyInfo.StrategyType == typeof(BasePathStrategy))
                {
                    context.Request.Path.StartsWithSegments("/" + tenant.Identifier, out var matched, out var newPath);
                    context.Request.PathBase = Path.Join(context.Request.PathBase, matched);
                    context.Request.Path = newPath;
                }
                await next.Invoke();
            });

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
                //endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
