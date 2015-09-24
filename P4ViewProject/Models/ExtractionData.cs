using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P4ViewProject.Models
{   
    public class ExtractionData
    {   
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExtractionID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SelectBox { get; set; }
        public string FromBox { get; set; }
        public string WhereBox { get; set; }
        public string OtherClausesBox { get; set; }
        public string QueryDate { get; set; }


    }
}