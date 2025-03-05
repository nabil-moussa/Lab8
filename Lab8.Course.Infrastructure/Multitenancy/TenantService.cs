using System.Collections.Concurrent;
using Lab8.Course.Common.Multitenancy;

namespace Lab8.Course.Infrastructure.Multitenancy;


public class TenantService : ITenantService
{
    private readonly ConcurrentDictionary<string, string> _tenants = new();

    public TenantService()
    {
        AddTenant("default", "public");
    }

    public void AddTenant(string branchId, string schemaName)
    {
        _tenants[branchId] = schemaName;
    }

    public string GetSchemaName(string branchId)
    {
        return _tenants.TryGetValue(branchId, out var schemaName) 
            ? schemaName 
            : "public";
    }

    public bool TenantExists(string branchId)
    {
        return _tenants.ContainsKey(branchId);
    }
}