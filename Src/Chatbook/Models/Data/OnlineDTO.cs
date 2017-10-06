using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FacebookClone.Models.Data
{
    [Table("tblOnline")]
    public class OnlineDTO
    {
        [Key]
        public int Id { get; set; }
        public string ConnId { get; set; }

        [ForeignKey("Id")]
        public virtual UserDTO Users { get; set; }
    }
}