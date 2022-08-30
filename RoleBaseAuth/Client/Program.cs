namespace RoleBaseAuth.Client
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using RoleBaseAuth.Shared;
    using BlazorTenant;
    using System.Collections.Generic;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Dictionary<string, string> CONNH = new Dictionary<string, string>();
            CONNH["Name"] = "Cats of Nashua";
            Tenant t = new Tenant ( "CONNH", CONNH);
            var store = new InMemoryTenantStore();
            store.TryAdd(t);


            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("RoleBaseAuth.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("RoleBaseAuth.ServerAPI"));
            builder.Services.AddScoped(typeof(AccountClaimsPrincipalFactory<RemoteUserAccount>), typeof(RolesAccountClaimsPrincipalFactory));
            builder.Services.AddApiAuthorization();
            builder.Services.AddAuthorizationCore(options => options.AddMarsPolicy());
            builder.Services.AddMultiTenantancy(store);
            var build = builder.Build();
            build.Services.AddServiceProviderToMultiTenantRoutes();
            await build.RunAsync();
        }
    }
}
