using Microsoft.EntityFrameworkCore;
using ViolationEditorApi.Models;

namespace ViolationEditorApi.context
{
    public class ViolationDbContext : DbContext
    {
        public ViolationDbContext(DbContextOptions<ViolationDbContext> options) : base(options)
        {
        }

        public DbSet<TblCSQ> TblCSQ { get; set; }
        public DbSet<TblREQ> TblREQ { get; set; }
        public DbSet<TblCallSurvey> TblCallSurvey { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ تعطيل OUTPUT بسبب التريجرات
            modelBuilder.Entity<TblCSQ>()
                .ToTable("tbl_CSQ")
                .HasAnnotation("SqlServer:UseOutputClause", false);

            // ✅ تعريف باقي الجداول بشكل صريح
            modelBuilder.Entity<TblREQ>().ToTable("tbl_REQ");
            modelBuilder.Entity<TblCallSurvey>().ToTable("tblCallSurvey");

            base.OnModelCreating(modelBuilder);
        }
    }
}
