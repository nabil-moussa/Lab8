namespace Lab8.Course.Common.Multitenancy;

public interface ITenantService
{
    void AddTenant(string branchId, string schemaName);
    string GetSchemaName(string branchId);
    bool TenantExists(string branchId);
}
