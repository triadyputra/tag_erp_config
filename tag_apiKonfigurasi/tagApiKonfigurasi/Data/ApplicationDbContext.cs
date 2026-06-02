using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using tagApiKonfigurasi.Model;
using tagApiKonfigurasi.Model.Konfigurasi;
using tagApiKonfigurasi.Model.MasterData;




namespace tagApiKonfigurasi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TblCabang> TblCabang { get; set; }
        public DbSet<TblConfiq> TblConfiq { get; set; }

        public DbSet<RefreshToken> RefreshToken { get; set; }
        
        public DbSet<AuditLogin> AuditLogin { get; set; }
        public DbSet<UserSession> UserSession { get; set; }



        public DbSet<MstModul> Moduls => Set<MstModul>();
        public DbSet<MstMenu> Menus => Set<MstMenu>();
        public DbSet<MstController> Controllers => Set<MstController>();
        public DbSet<MstAction> Actions => Set<MstAction>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================
            // MstMenu
            // =========================
            builder.Entity<MstMenu>(entity =>
            {
                entity.HasKey(x => x.IdMenu);

                entity.Property(x => x.IdMenu)
                    .HasMaxLength(50);

                entity.Property(x => x.NamaMenu)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.HasMany(x => x.Controllers)
                    .WithOne(x => x.Menu)
                    .HasForeignKey(x => x.IdMenu)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // MstController
            // =========================
            builder.Entity<MstController>(entity =>
            {
                entity.HasKey(x => x.IdController);

                entity.Property(x => x.IdController)
                    .HasMaxLength(50);

                entity.Property(x => x.NamaController)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.IdMenu)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasMany(x => x.Actions)
                    .WithOne(x => x.Controller)
                    .HasForeignKey(x => x.IdController)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // MstAction
            // =========================
            builder.Entity<MstAction>(entity =>
            {
                entity.HasKey(x => new { x.IdController, x.IdAction });

                entity.Property(x => x.IdAction)
                    .HasMaxLength(50);

                entity.Property(x => x.NamaAction)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.IdController)
                    .HasMaxLength(50)
                    .IsRequired();
            });
        }

    }
}
