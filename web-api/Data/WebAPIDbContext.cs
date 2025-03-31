using Microsoft.EntityFrameworkCore;
using web_api.Models;

namespace web_api.Data
{
    public class WebAPIDbContext : DbContext
    {
        public WebAPIDbContext(DbContextOptions<WebAPIDbContext> options)
            : base(options)
        {
        }

        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagAssignment> TagAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite primary key for the class TagAssignment, which is the linking class between Quote and Tag
            modelBuilder.Entity<TagAssignment>()
                .HasKey(ta => new { ta.QuoteId, ta.TagId });

            // Relationships
            modelBuilder.Entity<TagAssignment>()
                .HasOne(ta => ta.Quote)
                .WithMany(q => q.TagAssignments)
                .HasForeignKey(ta => ta.QuoteId);

            modelBuilder.Entity<TagAssignment>()
                .HasOne(ta => ta.Tag)
                .WithMany(t => t.TagAssignments)
                .HasForeignKey(ta => ta.TagId);
        }
    }
}
