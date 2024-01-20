using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using WpfApp.Entities;
using Wpf;

namespace WpfApp
{
    public class MovieContext: DbContext
    {
        public MovieContext() :base("DbConnection")
        { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Director> Directors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Movie>()
                .HasMany<Genre>(m => m.Genres)
                .WithMany(g => g.Movies)
                .Map(mg =>
                {
                    mg.MapLeftKey("MovieID");
                    mg.MapRightKey("GenreName");
                    mg.ToTable("MovieGenre");
                });

            
            modelBuilder.Entity<Movie>()
                .HasMany<Country>(m => m.Countries)
                .WithMany(c => c.Movies)
                .Map(mc =>
                {
                    mc.MapLeftKey("MovieID");
                    mc.MapRightKey("CountryName");
                    mc.ToTable("MovieCountry");
                });

            
            modelBuilder.Entity<Movie>()
                .HasMany<Director>(m => m.Directors)
                .WithMany(d => d.Movies)
                .Map(md =>
                {
                    md.MapLeftKey("MovieID");
                    md.MapRightKey("DirectorID");
                    md.ToTable("MovieDirector");
                });
        }

    }
}
