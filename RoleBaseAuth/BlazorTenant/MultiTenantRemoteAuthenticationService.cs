using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorTenant
{
    public class MultiTenantRemoteAuthenticationService : RemoteAuthenticationService<RemoteAuthenticationState, RemoteUserAccount, ApiAuthorizationProviderOptions>
    {
        protected string tenant = "";

        public MultiTenantRemoteAuthenticationService(IJSRuntime jsRuntime, IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> options, NavigationManager navigation, AccountClaimsPrincipalFactory<RemoteUserAccount> accountClaimsPrincipalFactory)
            : base(jsRuntime, options, navigation, accountClaimsPrincipalFactory)
        {
            try
            {
                // Ideally we would inject the BlazorTenant to get the tenant id, but at this stage
                // the service has not fully hooked-in and so we always get an empty Tenant object.
                // Therefore we have to infer the tenant from the startup page found in the navigation object.
                if (navigation.Uri.Length > 1)
                {
                    Uri uri = new Uri(navigation.Uri);
                    string[] segments = uri.Segments;
                    if (segments.Length > 1)
                    {
                        tenant = segments[1];
                        if (!tenant.EndsWith("/")) tenant += "/";

                        // Prefix all the relevant paths with the tenant id.  Presumes that the server is also multi-tenant.
                        Options.AuthenticationPaths.LogInCallbackPath = tenant + Options.AuthenticationPaths.LogInCallbackPath;
                        Options.AuthenticationPaths.LogInFailedPath = tenant + Options.AuthenticationPaths.LogInFailedPath;
                        Options.AuthenticationPaths.LogInPath = tenant + Options.AuthenticationPaths.LogInPath;
                        Options.AuthenticationPaths.LogOutCallbackPath = tenant + Options.AuthenticationPaths.LogOutCallbackPath;
                        Options.AuthenticationPaths.LogOutCallbackPath = tenant + Options.AuthenticationPaths.LogOutCallbackPath;
                        Options.AuthenticationPaths.LogOutFailedPath = tenant + Options.AuthenticationPaths.LogOutFailedPath;
                        Options.AuthenticationPaths.LogOutPath = tenant + Options.AuthenticationPaths.LogOutPath;
                        Options.AuthenticationPaths.LogOutSucceededPath = tenant + Options.AuthenticationPaths.LogOutSucceededPath;
                        Options.AuthenticationPaths.ProfilePath = tenant + Options.AuthenticationPaths.ProfilePath;
                        Options.AuthenticationPaths.RegisterPath = tenant + Options.AuthenticationPaths.RegisterPath;
                        Options.AuthenticationPaths.RemoteRegisterPath = tenant + Options.AuthenticationPaths.RemoteRegisterPath;
                        Options.AuthenticationPaths.RemoteProfilePath = tenant + Options.AuthenticationPaths.RemoteProfilePath;
                        Options.ProviderOptions.ConfigurationEndpoint = tenant + Options.ProviderOptions.ConfigurationEndpoint;
                    }
                }
            }
            catch
            {
            }
        }

        public override async Task<RemoteAuthenticationResult<RemoteAuthenticationState>> SignInAsync(RemoteAuthenticationContext<RemoteAuthenticationState> context)
        {
            context.State.ReturnUrl += tenant; // Added because the MultiTenantRouter removes the tenant from the current context
            var result = await base.SignInAsync(context);
            return result;
        }
    }
}
