using Microsoft.EntityFrameworkCore;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> movies { get; set; }
        public DbSet<Genre> genres { get; set; }
        public DbSet<Movie_Genre> movie_genre { get; set; }
        public DbSet<Actor> actors { get; set; }
        public DbSet<Movie_Actor> movie_actor { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie_Genre>()
                .Property(mg => mg.ID)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Movie_Actor>()
                .Property(ma => ma.ID)
                .ValueGeneratedOnAdd();
        }
    }
}
