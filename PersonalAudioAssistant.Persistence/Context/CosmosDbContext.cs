using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Domain.Entities;

namespace PersonalAudioAssistant.Persistence.Context
{
    public class CosmosDbContext : DbContext
    {
        public CosmosDbContext(DbContextOptions<CosmosDbContext> options) : base(options)
        {
        }

        public DbSet<MainUser> MainUsers { get; set; }
        public DbSet<SubUser> SubUsers { get; set; }
        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<IndividualParameters> IndividualParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MainUser>().ToContainer("User");
            modelBuilder.Entity<SubUser>().ToContainer("SubUser");
            modelBuilder.Entity<AppSettings>().ToContainer("AppSettings");
            modelBuilder.Entity<Conversation>().ToContainer("Conversation");
            modelBuilder.Entity<Message>().ToContainer("Message");
            modelBuilder.Entity<IndividualParameters>().ToContainer("IndividualParameters");
        }
    }
}
