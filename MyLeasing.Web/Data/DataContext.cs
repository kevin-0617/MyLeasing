using Microsoft.EntityFrameworkCore;
using MyLeasing.Web.Data.Entities;

namespace MyLeasing.Web.Data
{
    public class DataContext : DbContext
    {
        //conexion a la BD
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<Contract> contracts { get; set; }
        public DbSet<Lessee> Lessees { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> propertyImages { get; set; }
        public DbSet<PropertyType> propertyTypes { get; set; }

    }
}
