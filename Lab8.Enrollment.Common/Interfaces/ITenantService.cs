namespace Lab8.Enrollment.Common.Interfaces;

public interface ITenantService
{
    void AddTenant(string branchId, string schemaName);
    string GetSchemaName(string branchId);
    bool TenantExists(string branchId);
}
