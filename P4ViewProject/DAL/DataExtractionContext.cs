using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using P4ViewProject.Models;

namespace P4ViewProject.DAL
{
    public class DataExtractionContext : DbContext
    {
        public DataExtractionContext(): base("DataExtractionContext")
        {
            Database.SetInitializer<DataExtractionContext>(new CreateDatabaseIfNotExists<DataExtractionContext>());
        }

        public DbSet<ExtractionData> DataExtracts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}