namespace final.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class x1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Movies",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        Name = c.String(),
                        Synopsis = c.String(),
                        Year = c.Int(nullable: false),
                        Duration = c.Int(nullable: false),
                        Rate = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Movies");
        }
    }
}
