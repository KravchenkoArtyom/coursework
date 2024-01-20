namespace final.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteRowVersion : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Movies", "RowVersion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Movies", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
        }
    }
}
