namespace DefaultNamespace;

public class TripsDbContext : DbContext
{
    public TripsDbContext(DbContextOptions<TripsDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<CountryTrip> CountryTrips { get; set; }
    public DbSet<ClientTrip> ClientTrips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CountryTrip>()
            .HasKey(ct => new { ct.IdCountry, ct.IdTrip });

        modelBuilder.Entity<ClientTrip>()
            .HasKey(ct => new { ct.IdClient, ct.IdTrip });
    }
}