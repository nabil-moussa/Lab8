using Lab8.Enrollment.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Lab8.Enrollment.Domain.Models;

namespace Lab8.Enrollment.Persistence.Context
{
    public class EnrollmentDbContext : DbContext
    {
        private readonly ITenantService _tenantService;
        private string _currentBranchId = "default";

        public EnrollmentDbContext(
            DbContextOptions<EnrollmentDbContext> options,
            ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Domain.Models.Enrollment> Enrollments { get; set; }

        public void SetBranchId(string branchId)
        {
            _currentBranchId = branchId ?? "default";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filter for branch-specific data
            modelBuilder.Entity<Domain.Models.Enrollment>().HasQueryFilter(
                e => e.BranchId == _currentBranchId
            );

            // Configure schema based on current tenant
            modelBuilder.HasDefaultSchema(_tenantService.GetSchemaName(_currentBranchId));

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnrollmentDbContext).Assembly);
        }

        public async Task EnsureSchemaCreatedAsync(string branchId)
        {
            var schemaName = _tenantService.GetSchemaName(branchId);
            
            // Create schema if not exists
            await Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS {schemaName}");
            
            // Migrate the schema
            await Database.MigrateAsync();
        }

        public override int SaveChanges()
        {
            // Automatically set BranchId for new entities
            foreach (var entry in ChangeTracker.Entries<Domain.Models.Enrollment>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.BranchId = _currentBranchId;
                    entry.Entity.SchemaName = _tenantService.GetSchemaName(_currentBranchId);
                }
            }
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set BranchId for new entities
            foreach (var entry in ChangeTracker.Entries<Domain.Models.Enrollment>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.BranchId = _currentBranchId;
                    entry.Entity.SchemaName = _tenantService.GetSchemaName(_currentBranchId);
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}