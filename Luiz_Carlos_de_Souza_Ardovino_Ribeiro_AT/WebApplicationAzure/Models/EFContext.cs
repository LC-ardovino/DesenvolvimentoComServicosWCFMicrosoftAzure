using Microsoft.EntityFrameworkCore;
using WebApplicationAzure.Models;
using App.BLL.Models;

namespace WebAppMVC.EFCore.Models
{
    public class EFContext : DbContext
    {
        private string connectionString;

        public EFContext(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }


        public DbSet<Friend>? Friends { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Friend>()
                .ToTable("Friends")
                .HasKey(p => p.Id);

            modelBuilder.Entity<Friend>()
               .Property(p => p.Id)
               .ValueGeneratedOnAdd();

            modelBuilder.Entity<Friend>()
                .Property(p => p.FirstName)
                .HasColumnType("VARCHAR(80)")
                .IsRequired();

            modelBuilder.Entity<Friend>()
                .Property(p => p.LastName)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();

            modelBuilder.Entity<Friend>()
                .Property(p => p.BirthDate)
                .HasColumnType("datetime");

            modelBuilder.Entity<Friend>()
               .Property(p => p.Views)
               .HasColumnType("int")
               .IsRequired();

            modelBuilder.Entity<Friend>()
               .Property(p => p.PictureUrl)
               .HasColumnType("VARCHAR(1000)")
               .IsRequired(false);


        }

    }
}
