using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using UcbBack.Logic.B1;
using UcbBack.Models;
using UcbBack.Models.Auth;
using UcbBack.Models.Dist;
using UcbBack.Models.Serv;

namespace UcbBack.Logic.ExcelFiles.Serv
{
    public class Serv_VariosExcel : ValidateExcelFile
    {
        private static Excelcol[] cols = new[]
        {
            new Excelcol("Codigo Socio", typeof(string)), 
            new Excelcol("Nombre Socio", typeof(string)),
            new Excelcol("Cod Dependencia", typeof(string)),
            new Excelcol("PEI PO", typeof(string)),
            new Excelcol("Nombre del Servicio", typeof(string)),
            new Excelcol("Objeto del Contrato", typeof(string)),
            new Excelcol("Cuenta Asignada", typeof(string)),
            new Excelcol("Monto Contrato", typeof(double)),
            new Excelcol("Monto IUE", typeof(double)),
            new Excelcol("Monto IT", typeof(double)),
            new Excelcol("Monto a Pagar", typeof(double)),
            new Excelcol("Observaciones", typeof(string)),
        };

        private ApplicationDbContext _context;
        private ServProcess process;
        private CustomUser user;

        public Serv_VariosExcel(string fileName, int headerin = 1)
            : base(cols, fileName, headerin)
        { }

        public Serv_VariosExcel(Stream data, ApplicationDbContext context, string fileName, ServProcess process, CustomUser user, int headerin = 1, int sheets = 1, string resultfileName = "Result") 
            : base(cols, data, fileName, headerin, sheets, resultfileName, context)
        {
            this.user = user;
            this.process = process;
            _context = context;
            isFormatValid();
        }

        public override void toDataBase()
        {
            IXLRange UsedRange = wb.Worksheet(1).RangeUsed();

            for (int i = 1 + headerin; i <= UsedRange.LastRow().RowNumber(); i++)
            {
                _context.ServVarioses.Add(ToServVarios(i));
            }

            _context.SaveChanges();
        }

        public Serv_Varios ToServVarios(int row, int sheet = 1)
        {
            Serv_Varios data = new Serv_Varios();
            data.Id = Serv_Varios.GetNextId(_context);

            data.CardCode = wb.Worksheet(sheet).Cell(row, 1).Value.ToString();
            data.CardName = wb.Worksheet(sheet).Cell(row, 2).Value.ToString();
            var cod = wb.Worksheet(sheet).Cell(row, 3).Value.ToString();
            var depId = _context.Dependencies
                .FirstOrDefault(x => x.Cod == cod);
            data.DependencyId = depId.Id;
            data.PEI = wb.Worksheet(sheet).Cell(row, 4).Value.ToString();
            data.ServiceName = wb.Worksheet(sheet).Cell(row, 5).Value.ToString();
            data.ContractObjective = wb.Worksheet(sheet).Cell(row, 6).Value.ToString();
            data.AssignedAccount = wb.Worksheet(sheet).Cell(row, 7).Value.ToString();
            data.ContractAmount = Math.Round(Decimal.Parse(wb.Worksheet(sheet).Cell(row, 8).Value.ToString()), 2);
            data.IUE = Math.Round(Decimal.Parse(wb.Worksheet(sheet).Cell(row, 9).Value.ToString()), 2);
            data.IT = Math.Round(Decimal.Parse(wb.Worksheet(sheet).Cell(row, 10).Value.ToString()), 2);
            data.TotalAmount = Math.Round(Decimal.Parse(wb.Worksheet(sheet).Cell(row, 11).Value.ToString()), 2);
            data.Comments = wb.Worksheet(sheet).Cell(row, 12).Value.ToString();
            data.Serv_ProcessId = process.Id;
            return data;
        }

        public override bool ValidateFile()
        {
            if (isValid())
            {
                var connB1 = B1Connection.Instance();

                if (!connB1.connectedtoHana)
                {
                    addError("Error en SAP", "No se puedo conectar con SAP B1, es posible que algunas validaciones cruzadas con SAP no sean ejecutadas");
                }

                bool v1 = VerifyBP(1, 2,process.BranchesId,user);
                bool v2 = VerifyColumnValueIn(3, _context.Dependencies.Where(x => x.BranchesId == this.process.BranchesId).Select(x => x.Cod).ToList(), comment: "Esta Dependencia no es Válida");
                var pei = connB1.getCostCenter(B1Connection.Dimension.PEI).Cast<string>().ToList();
                bool v3 = VerifyColumnValueIn(4, pei, comment: "Este PEI no existe en SAP.");
                bool v4 = VerifyLength(5, 50);
                bool v5 = VerifyLength(6, 50);
                bool v6 = VerifyColumnValueIn(7, new List<string> { "CC_ACADEMICA", "CC_SOCIAL", "CC_DEPORTIVA", "CC_CULTURAL", "CC_PASTORAL", "CC_OTROS", "CC_TEMPORAL" }, comment: "No existe este tipo de Cuenta Asignada.");
                bool v7 = VerifyTotal();
                bool v8 = true;
                foreach (var i in new List<int>() { 1,2,3,4,5,6,7,8,9,10,11})
                {
                    v8 = VerifyNotEmpty(i) && v8 ;
                }

                return v1 && v2 && v3 && v4 && v5 && v6 && v7 && v8;
            }

            return false;
        }

        private bool VerifyTotal()
        {
            bool res = true;
            int sheet = 1;

            IXLRange UsedRange = wb.Worksheet(sheet).RangeUsed();
            for (int i = headerin + 1; i <= UsedRange.LastRow().RowNumber(); i++)
            {

                string SRTcontrato = wb.Worksheet(sheet).Cell(i, 8).Value.ToString();
                string SRTIUE = wb.Worksheet(sheet).Cell(i, 9).Value.ToString();
                string SRTIT = wb.Worksheet(sheet).Cell(i, 10).Value.ToString();
                string SRTtotal = wb.Worksheet(sheet).Cell(i, 11).Value.ToString();
                
                decimal contrato = Math.Round(Decimal.Parse(SRTcontrato), 2);
                decimal IUE = Math.Round(Decimal.Parse(SRTIUE), 2);
                decimal IT = Math.Round(Decimal.Parse(SRTIT), 2);
                decimal total = Math.Round(Decimal.Parse(SRTtotal), 2);

                if (contrato-IUE-IT != total)
                {
                    res = false;
                    paintXY(11, i, XLColor.Red, "Este valor no cuadra (Contrato - IUE - IT != Monto a Pagar)");
                }
            }

            valid = valid && res;
            if (!res)
                addError("Valor no valido", "Monto a Pagar no cuadra." , false);
            return res;
        }

    }
}