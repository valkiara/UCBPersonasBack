﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using UcbBack.Logic.B1;
using UcbBack.Models;

namespace UcbBack.Logic.ExcelFiles
{
    public class ORExcel : ValidateExcelFile
    {
        private static Excelcol[] cols = new[]
        {
            new Excelcol("Carnet Identidad", typeof(string)),
            new Excelcol("Primer Apellido", typeof(string)),
            new Excelcol("Segundo Apellido", typeof(string)),
            new Excelcol("Nombres", typeof(string)),
            new Excelcol("Apellido Casada", typeof(string)),
            new Excelcol("Segmento origen", typeof(string)),
            new Excelcol("Total Neto Ganado", typeof(double)),
            new Excelcol("CUNI", typeof(string)),
            new Excelcol("CCD1", typeof(string)),
            new Excelcol("CCD2", typeof(string)),
            new Excelcol("CCD3", typeof(string)),
            new Excelcol("CCD4", typeof(string)),
            new Excelcol("CCD5", typeof(string)),
            new Excelcol("CCD6", typeof(string)),
        };
        private ApplicationDbContext _context;
        private string mes, gestion, segmentoOrigen;
        private Dist_File file;
        public ORExcel(Stream data, ApplicationDbContext context, string fileName, string mes, string gestion, string segmentoOrigen,Dist_File file, int headerin = 3, int sheets = 1, string resultfileName = "Result")
            : base(cols, data, fileName, headerin, sheets, resultfileName)
        {
            this.segmentoOrigen = segmentoOrigen;
            this.gestion = gestion;
            this.mes = mes;
            this.file = file;
            _context = context;
            isFormatValid();
        }
        public ORExcel(string fileName, int headerin = 1)
            : base(cols, fileName, headerin)
        { }

        public override void toDataBase()
        {
            IXLRange UsedRange = wb.Worksheet(1).RangeUsed();

            for (int i = 1 + headerin; i <= UsedRange.LastRow().RowNumber(); i++)
            {
                _context.DistOrs.Add(ToDistDiscounts(i));
            }

            _context.SaveChanges();
        }

        public override bool ValidateFile()
        {
            var connB1 = B1Connection.Instance();
            bool v1 = VerifyColumnValueIn(6,_context.Branch.Select(x=>x.Abr).ToList(),comment:"Esta Regional No existe");
            bool v2 = VerifyPerson(ci: 1, CUNI: 8, fullname: 2, personActive: false);
            //Dim1
            var D1 = connB1.getCostCenter(B1Connection.Dimension.OrganizationalUnit, mes: this.mes, gestion: this.gestion).Cast<string>().ToList();
            D1.Add("");
            bool v8 = VerifyColumnValueIn(9, D1, comment: "Esta Unidad Organizacional no existe en SAP.");
            // PEI
            var pei = connB1.getCostCenter(B1Connection.Dimension.PEI, mes: this.mes, gestion: this.gestion).Cast<string>().ToList();
            pei.Add("");
            bool v3 = VerifyColumnValueIn(10, pei, comment: "Este PEI no existe en SAP.");
            //Plan Acad
            var planacad = connB1.getCostCenter(B1Connection.Dimension.PlanAcademico, mes: this.mes, gestion: this.gestion).Cast<string>().ToList();
            planacad.Add("");
            bool v4 = VerifyColumnValueIn(11, planacad, comment: "Este plan de estudios no existe en SAP.");
            //paralelo
            var paralelo = connB1.getCostCenter(B1Connection.Dimension.Paralelo, mes: this.mes, gestion: this.gestion).Cast<string>().ToList();
            paralelo.Add("");
            bool v5 = VerifyColumnValueIn(12, paralelo, comment: "Este paralelo no existe en SAP.");
            //periodo
            var periodo = connB1.getCostCenter(B1Connection.Dimension.Periodo, mes: this.mes, gestion: this.gestion).Cast<string>().ToList();
            periodo.Add("");
            bool v6 = VerifyColumnValueIn(13, periodo, comment: "Este periodo no existe en SAP.");
            //proyectos
            var projects = connB1.getProjects().Cast<String>().ToList();
            projects.Add("");
            bool v7 = VerifyColumnValueIn(14, projects, comment: "Este proyecto no existe en SAP.");

            return isValid() && v1 && v2 && v3 && v4 && v5 && v6 && v7 && v8;
        }

        public Dist_OR ToDistDiscounts(int row, int sheet = 1)
        {
            Dist_OR dis = new Dist_OR();
            dis.Id = _context.Database.SqlQuery<int>("SELECT ADMNALRRHH.\"rrhh_Dist_OR_sqs\".nextval FROM DUMMY;").ToList()[0];
            dis.Document = wb.Worksheet(sheet).Cell(row, 1).Value.ToString();
            dis.Names = wb.Worksheet(sheet).Cell(row, 2).Value.ToString();
            dis.FirstSurName = wb.Worksheet(sheet).Cell(row, 3).Value.ToString();
            dis.SecondSurName = wb.Worksheet(sheet).Cell(row, 4).Value.ToString();
            dis.MariedSurName = wb.Worksheet(sheet).Cell(row, 5).Value.ToString();
            dis.segmento = wb.Worksheet(sheet).Cell(row, 6).Value.ToString();
            dis.TotalGanado = strToDecimal(row,7);
            dis.CUNI = wb.Worksheet(sheet).Cell(row, 8).Value.ToString();
            dis.Dependency = wb.Worksheet(sheet).Cell(row, 9).Value.ToString();
            dis.PEI = wb.Worksheet(sheet).Cell(row, 10).Value.ToString();
            dis.PlanEstudios = wb.Worksheet(sheet).Cell(row, 11).Value.ToString();
            dis.Paralelo = wb.Worksheet(sheet).Cell(row, 12).Value.ToString();
            dis.Periodo = wb.Worksheet(sheet).Cell(row, 13).Value.ToString();
            dis.Project = wb.Worksheet(sheet).Cell(row, 14).Value.ToString();

            dis.Porcentaje = 0m;
            dis.mes = this.mes;
            dis.gestion = this.gestion;
            dis.segmentoOrigen = this.segmentoOrigen;

            dis.DistFileId = file.Id;
            return dis;
        }
    }
}