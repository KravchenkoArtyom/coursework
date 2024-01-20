namespace final.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateEntities : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Movies");
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        CountryName = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.CountryName);
            
            CreateTable(
                "dbo.Directors",
                c => new
                    {
                        DirectorID = c.Guid(nullable: false),
                        DirectorName = c.String(),
                    })
                .PrimaryKey(t => t.DirectorID);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        GenreName = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.GenreName);
            
            CreateTable(
                "dbo.MovieCountry",
                c => new
                    {
                        MovieID = c.Guid(nullable: false),
                        CountryName = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => new { t.MovieID, t.CountryName })
                .ForeignKey("dbo.Movies", t => t.MovieID, cascadeDelete: true)
                .ForeignKey("dbo.Countries", t => t.CountryName, cascadeDelete: true)
                .Index(t => t.MovieID)
                .Index(t => t.CountryName);
            
            CreateTable(
                "dbo.MovieDirector",
                c => new
                    {
                        MovieID = c.Guid(nullable: false),
                        DirectorID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MovieID, t.DirectorID })
                .ForeignKey("dbo.Movies", t => t.MovieID, cascadeDelete: true)
                .ForeignKey("dbo.Directors", t => t.DirectorID, cascadeDelete: true)
                .Index(t => t.MovieID)
                .Index(t => t.DirectorID);
            
            CreateTable(
                "dbo.MovieGenre",
                c => new
                    {
                        MovieID = c.Guid(nullable: false),
                        GenreName = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => new { t.MovieID, t.GenreName })
                .ForeignKey("dbo.Movies", t => t.MovieID, cascadeDelete: true)
                .ForeignKey("dbo.Genres", t => t.GenreName, cascadeDelete: true)
                .Index(t => t.MovieID)
                .Index(t => t.GenreName);
            
            AddColumn("dbo.Movies", "MovieID", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.Movies", "MovieID");
            DropColumn("dbo.Movies", "ID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Movies", "ID", c => c.Guid(nullable: false));
            DropForeignKey("dbo.MovieGenre", "GenreName", "dbo.Genres");
            DropForeignKey("dbo.MovieGenre", "MovieID", "dbo.Movies");
            DropForeignKey("dbo.MovieDirector", "DirectorID", "dbo.Directors");
            DropForeignKey("dbo.MovieDirector", "MovieID", "dbo.Movies");
            DropForeignKey("dbo.MovieCountry", "CountryName", "dbo.Countries");
            DropForeignKey("dbo.MovieCountry", "MovieID", "dbo.Movies");
            DropIndex("dbo.MovieGenre", new[] { "GenreName" });
            DropIndex("dbo.MovieGenre", new[] { "MovieID" });
            DropIndex("dbo.MovieDirector", new[] { "DirectorID" });
            DropIndex("dbo.MovieDirector", new[] { "MovieID" });
            DropIndex("dbo.MovieCountry", new[] { "CountryName" });
            DropIndex("dbo.MovieCountry", new[] { "MovieID" });
            DropPrimaryKey("dbo.Movies");
            DropColumn("dbo.Movies", "MovieID");
            DropTable("dbo.MovieGenre");
            DropTable("dbo.MovieDirector");
            DropTable("dbo.MovieCountry");
            DropTable("dbo.Genres");
            DropTable("dbo.Directors");
            DropTable("dbo.Countries");
            AddPrimaryKey("dbo.Movies", "ID");
        }
    }
}
