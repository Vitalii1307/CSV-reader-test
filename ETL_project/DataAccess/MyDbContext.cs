using ETL_project.Enities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL_project.DataAccess
{
    public class MyDbContext : DbContext
    {
        public DbSet<CabTrip> CabTrips { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Database=etl_db;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define table schema and data types here

            modelBuilder.Entity<CabTrip>(entity =>
            {
                // Define primary key
                entity.HasKey(e => e.Id);

                // Define column properties and data types
                entity.Property(e => e.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
                entity.Property(e => e.PickupDateTime).HasColumnName("PickupDateTime").HasColumnType("datetime2").IsRequired();
                entity.Property(e => e.DropoffDateTime).HasColumnName("DropoffDateTime").HasColumnType("datetime2").IsRequired();
                entity.Property(e => e.PassengerCount).HasColumnName("PassengerCount").HasColumnType("int").IsRequired();
                entity.Property(e => e.TripDistance).HasColumnName("TripDistance").HasColumnType("float").IsRequired();
                entity.Property(e => e.StoreAndFwdFlag).HasColumnName("StoreAndFwdFlag").HasColumnType("nvarchar(3)").IsRequired();
                entity.Property(e => e.PULocationID).HasColumnName("PULocationID").HasColumnType("int").IsRequired();
                entity.Property(e => e.DOLocationID).HasColumnName("DOLocationID").HasColumnType("int").IsRequired();
                entity.Property(e => e.FareAmount).HasColumnName("FareAmount").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.TipAmount).HasColumnName("TipAmount").HasColumnType("decimal(10,2)").IsRequired();
            });
        }
    }
}
