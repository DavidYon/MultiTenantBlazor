using System;
using Microsoft.Extensions.DependencyInjection;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RoleBaseAuth.Server.Pages
{
    public class _HostModel : PageModel
    {
        public TenantInfo Tenant { get; private set; }
        public _HostModel(IServiceProvider services)
        {
            try
            {
                Tenant = services.GetService<TenantInfo>();
            }
            catch
            {
            }
            finally
            {
                if (Tenant == null)
                {
                    Tenant = new TenantInfo
                    {
                        Id = "Non_xxxxxx",
                        Identifier = "",
                        Name = "No Tenant"
                    };
                }
            }
        }

        public void OnGet()
        {
        }
    }
}
