namespace P4ViewProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExtractionName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExtractionData", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExtractionData", "Name");
        }
    }
}
