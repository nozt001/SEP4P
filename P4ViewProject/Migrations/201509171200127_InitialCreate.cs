namespace P4ViewProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExtractionData",
                c => new
                    {
                        ExtractionID = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        SelectBox = c.String(),
                        FromBox = c.String(),
                        WhereBox = c.String(),
                        OtherClausesBox = c.String(),
                        QueryDate = c.String(),
                    })
                .PrimaryKey(t => t.ExtractionID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExtractionData");
        }
    }
}
