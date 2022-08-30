// https://github.com/dotnet/aspnetcore/blob/main/src/Components/Components/src/Routing/RouteTable.cs

namespace BlazorTenant
{
    internal class MultiTenantRouteTable : IRouteTable
    {
        public MultiTenantRouteTable(MultiTenantRouteEntry[] routes)
        {
            Routes = routes;
        }

        public MultiTenantRouteEntry[] Routes { get; }

        public void Route(MultiTenantRouteContext routeContext)
        {
            for(var i = 0; i< Routes.Length; i++)
            {
                Routes[i].Match(routeContext);
                if(routeContext.Handler != null)
                    return;
            }
        }

        public bool MatchesRoute(string tenantIdentifier)
        {
            for(var i = 0; i < Routes.Length; i++)
            {
                if(Routes[i].Match(tenantIdentifier))
                {
                    return true;
                }
            }

            return false;
        }
    }
}