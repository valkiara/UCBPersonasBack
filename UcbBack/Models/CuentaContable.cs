﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UcbBack.Models
{
    [Table("ADMNALRRHH.CuentasContables")]
    public class CuentaContable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { set; get; }

        public GrupoContable GrupoContable { get; set; }
        public int GrupoContableId { get; set; }
        public string Concept { get; set; }
        public Branches Branches { get; set; }
        public int BranchesId { get; set; }
        public string Name { get; set; }
        public string Indicator { get; set; }
    }
}