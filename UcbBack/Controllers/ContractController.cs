﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using UcbBack.Models;
using ExcelDataReader;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UcbBack.Logic;
using UcbBack.Logic.ExcelFiles;
using System.Globalization;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json.Linq;


namespace UcbBack.Controllers
{
    public class ContractController : ApiController
    {
         private ApplicationDbContext _context;
        private ValidateAuth auth;

        public ContractController()
        {

            _context = new ApplicationDbContext();
            auth = new ValidateAuth();

        }

        // GET api/Contract
        [Route("api/Contract")]
        public IHttpActionResult Get()
        {
            DateTime date = DateTime.Now;
            var contplist = _context.ContractDetails
                .Include(p => p.Branches)
                .Include(p => p.Dependency)
                .Include(p => p.Positions)
                .Include(p => p.People)
                .Where(x => /*x.StartDate <= date
                            && */(x.EndDate == null || x.EndDate > date))
                .OrderByDescending(x=>x.StartDate)
                .ToList()
                .Select(x => new
                {
                    x.Id, 
                    x.People.CUNI, 
                    x.People.Document, 
                    FullName= x.People.GetFullName(),
                    Dependency = x.Dependency.Name, 
                    DependencyCod = x.Dependency.Cod, 
                    Branches = x.Branches.Abr, 
                    BranchesId = x.Branches.Id, 
                    Positions=x.Positions.Name, 
                    x.Dedication,
                    x.Linkage,
                    StartDate = x.StartDate.ToString("dd MMM yyyy", new CultureInfo("es-ES")),
                    EndDate = x.EndDate == null?null:x.EndDate.GetValueOrDefault().ToString("dd MMM yyyy", new CultureInfo("es-ES"))
                }).ToList();
            var user = auth.getUser(Request);

            var res = auth.filerByRegional(contplist.AsQueryable(), user);

            return Ok(res);
        }

        [HttpGet]
        [Route("api/ContractSAP")]
        public IHttpActionResult GetSAP()
        {
            DateTime date = DateTime.Now;
            var contplist = _context.ContractDetails
                .Include(p => p.Branches)
                .Include(p => p.Dependency)
                .Include(p => p.Positions)
                .Include(p => p.People).ToList()
                .Where(x => /*x.StartDate <= date
                            && */(x.EndDate == null || x.EndDate.Value.Year * 100 + x.EndDate.Value.Month >= date.Year * 100 + date.Month))
                .OrderByDescending(x => x.StartDate)
                .ToList()
                .Select(x => new
                {
                    x.People.CUNI,
                    x.People.Document,
                    FullName = x.People.GetFullName(),
                    Dependency = x.Dependency.Name,
                    DependencyCod = x.Dependency.Cod,
                    x.BranchesId
                }).ToList();
            var user = auth.getUser(Request);

            var res = auth.filerByRegional(contplist.AsQueryable(), user);

            return Ok(res);
        }

        // GET api/Contract
        [Route("api/Contract/{id}")]
        public IHttpActionResult GetContract(int id)
        {
            DateTime date = DateTime.Now;
            var contplist = _context.ContractDetails
                .Include(p => p.Branches)
                .Include(p => p.Dependency)
                .Include(p => p.Positions)
                .Include(p => p.People)
                .Where(x =>/* x.StartDate <= date
                            &&*/ (x.EndDate == null || x.EndDate > date)
                            && x.Id == id)
                .OrderByDescending(x => x.StartDate)
                .ToList()
                .Select(x => new
                {
                    x.Id,
                    x.People.CUNI,
                    x.People.Document,
                    FullName = x.People.GetFullName(),
                    Dependency = x.Dependency.Name,
                    Branches = x.Branches.Abr,
                    BranchesId = x.Branches.Id,
                    Positions = x.Positions.Name,
                    x.Dedication,
                    x.Linkage,
                    StartDate = x.StartDate.ToString("dd MMM yyyy", new CultureInfo("es-ES")),
                    EndDate = x.EndDate == null ? null : x.EndDate.GetValueOrDefault().ToString("dd MMM yyyy", new CultureInfo("es-ES"))
                });

            var user = auth.getUser(Request);
            var res = auth.filerByRegional(contplist.AsQueryable(), user);
            if (res.Count() == 0)
                return NotFound();

            return Ok(res.FirstOrDefault());
        }

        [HttpGet]
        [Route("api/Contract/GetPersonContract/{id}")]
        public IHttpActionResult GetPersonContract(int id)
        {
            List<ContractDetail> contractInDB = null;

            contractInDB = _context.ContractDetails.Where(d => d.People.Id == id).ToList();

            if (contractInDB == null)
                return NotFound();

            return Ok(contractInDB);
        }
        [HttpGet]
        [Route("api/Contract/GetContractsBranch/{id}")]
        public IHttpActionResult GetContractsBranch(int id)
        {
            List<ContractDetail> contractInDB = null;
            DateTime date=new DateTime(2018,9,1);
            DateTime date2=new DateTime(2018,9,30);
            var people = _context.ContractDetails.Include(x=>x.People).Include(x=>x.Branches).Where(x=>  (x.EndDate==null || x.EndDate>date2)).Select(x=>x.People).Distinct();
            // var people = _context.CustomUsers.Include(x => x.People).Select(x => x.People);
            int i = people.Count();
            string res = "";

            foreach (var person in people)
            {
                var contract = person.GetLastContract();
                var user = _context.CustomUsers.FirstOrDefault(x => x.PeopleId == contract.People.Id);
               /* res += contract.People.GetFullName() + ";";
                res += user.UserPrincipalName + ";";
                res += "NORMAL;";
                res += "NO;";
                res += contract.Branches.Abr + ";";
                res += "RENDICIONES;";
                res += contract.CUNI + ";";*/

                res += contract.People.CUNI + ";";
                res += contract.People.Document + ";";
                res += contract.People.GetFullName() + ";";
                res += contract.People.FirstSurName + ";";
                res += contract.People.SecondSurName + ";";
                res += contract.People.MariedSurName + ";";
                res += contract.People.Names + ";";
                res += contract.People.BirthDate + ";";


                res += contract.Dependency.Cod + ";";
                res += contract.Dependency.Name + ";";

                res += contract.Dependency.OrganizationalUnitId + ";";

                res += contract.Positions.Name + ";";
                res += contract.Dedication + ";";
                res += contract.Linkage + ";";
                res += contract.AI + ";";


                res += contract.Branches.Abr + ";";
                res += contract.Branches.Name;

                res += "\n";
            }


            return Ok(res);
        }

        [NonAction]
        public async Task<System.Dynamic.ExpandoObject> HttpContentToVariables(MultipartMemoryStreamProvider req)
        {
            dynamic res = new System.Dynamic.ExpandoObject();
            foreach (HttpContent contentPart in req.Contents)
            {
                var contentDisposition = contentPart.Headers.ContentDisposition;
                string varname = contentDisposition.Name;
                if (varname == "\"segmentoOrigen\"")
                {
                    res.segmentoOrigen = contentPart.ReadAsStringAsync().Result;
                }
                else if (varname == "\"file\"")
                {
                    Stream stream = await contentPart.ReadAsStreamAsync();
                    res.fileName = String.IsNullOrEmpty(contentDisposition.FileName) ? "" : contentDisposition.FileName.Trim('"');
                    res.excelStream = stream;
                }
            }
            return res;
        }

        [HttpGet]
        [Route("api/Contract/AltaExcel/save/{id}")]
        public IHttpActionResult saveLastAltaExcel(int id)
        {
            var tempAlta = _context.TempAltas.Where(x => x.BranchesId == id && x.State != "SAVED");
            if (tempAlta.Count() > 0)
                return NotFound();

            var validator = new ValidatePerson();

            foreach (var alta in tempAlta)
            {
                var person = new People();

                if (alta.State == "NEW")
                {
                    person.Id = People.GetNextId(_context);
                    person.FirstSurName = alta.FirstSurName.Trim();
                    person.SecondSurName = alta.SecondSurName.Trim().IsNullOrWhiteSpace() ? null : alta.SecondSurName.Trim();
                    person.MariedSurName = alta.MariedSurName.Trim().IsNullOrWhiteSpace() ? null : alta.MariedSurName.Trim();
                    person.Names = alta.Names.Trim();
                    person.BirthDate = alta.BirthDate;
                    person.Gender = alta.Gender;

                    person.AFP = alta.AFP;
                    person.NUA = alta.NUA;

                    person.Document = alta.Document;
                    person.Ext = alta.Ext;
                    person.TypeDocument = alta.TypeDocument;

                    person.UseSecondSurName = person.SecondSurName.IsNullOrWhiteSpace();
                    person.UseMariedSurName = person.MariedSurName.IsNullOrWhiteSpace();

                    person = validator.UcbCode(person);
                    person.Pending = true;

                    _context.Person.Add(person);
                }
                else
                {
                    person = _context.Person.FirstOrDefault(x => x.CUNI == alta.CUNI);
                }

                var contract = new ContractDetail();

                contract.Id = ContractDetail.GetNextId(_context);
                contract.DependencyId = _context.Dependencies.FirstOrDefault(x=>x.Cod==alta.Dependencia).Id;
                contract.CUNI = person.CUNI;
                contract.PeopleId = person.Id;
                contract.BranchesId = alta.BranchesId;
                contract.Dedication = "TH";
                contract.Linkage = "TH";
                contract.PositionDescription = "Docente Tiempo Horario";
                contract.PositionsId = 26;
                contract.StartDate = alta.StartDate;
                contract.EndDate = alta.EndDate;

            }

            _context.SaveChanges();
            return Ok(tempAlta);
        }


        [HttpDelete]
        [Route("api/Contract/AltaExcel")]
        public IHttpActionResult removeLastAltaExcel(JObject data)
        {
            int branchesid;
            if (data["segmentoOrigen"] == null || !Int32.TryParse(data["segmentoOrigen"].ToString(), out branchesid))
            {
                ModelState.AddModelError("Mal Formato", "Debes enviar mes, gestion y segmentoOrigen");
                return BadRequest();

            }
            List<TempAlta> tempAlta = _context.TempAltas.Where(x => x.BranchesId == branchesid && x.State != "UPLOADED" && x.State != "CANCELED").ToList();
            foreach (var al in tempAlta)
            {
                al.State = "CANCELED";
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        [Route("api/Contract/AltaExcel/{id}")]
        public IHttpActionResult getLastAltaExcel(int id)
        {
            List<TempAlta> tempAlta = _context.TempAltas.Where(x => x.BranchesId == id && x.State != "UPLOADED" && x.State != "CANCELED").ToList();
            return Ok(tempAlta);
        }

        [HttpGet]
        [Route("api/Contract/AltaExcel")]
        public HttpResponseMessage getAltaExcelTemplate()
        {
            ContractExcel contractExcel = new ContractExcel(fileName: "AltaExcel_TH.xlsx", headerin: 3);
            return contractExcel.getTemplate();
        }

        [HttpPost]
        [Route("api/Contract/AltaExcel")]
        public async Task<HttpResponseMessage> AltaExcel()
        {
            var response = new HttpResponseMessage();
            try
            {
                var req = await Request.Content.ReadAsMultipartAsync();
                dynamic o = HttpContentToVariables(req).Result;
                int segment = 0;
                if (o.segmentoOrigen == null || !Int32.TryParse(o.segmentoOrigen,out segment))
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar segmentoOrigen");
                    return response;
                }

                var segId = _context.Branch.FirstOrDefault(b => b.Id == segment);
                if (segId == null)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Content = new StringContent("Debe enviar segmentoOrigen valido");
                    return response;
                }
                ContractExcel contractExcel = new ContractExcel(o.excelStream, _context, o.fileName, segId.Id, headerin: 3, sheets: 1);
                if (contractExcel.ValidateFile())
                {
                    contractExcel.toDataBase();
                    response.StatusCode = HttpStatusCode.OK;
                    response.Content = new StringContent("Se subio el archivo correctamente.");
                    return response;
                }
                return contractExcel.toResponse();
            }
            catch (System.ArgumentException e)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent("Por favor enviar un archivo en formato excel (.xls, .xslx)" + e);
                return response;
            }
        }

        //altas
        // POST api/Contract/alta/4
        [HttpPost]
        [Route("api/Contract/Alta")]
        public IHttpActionResult Post([FromBody]ContractDetail contract)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            People person = _context.Person.FirstOrDefault(x => x.CUNI == contract.CUNI.ToString());

            contract.PeopleId = person.Id;
            contract.CUNI = person.CUNI;


            contract.Id = ContractDetail.GetNextId(_context);

            _context.ContractDetails.Add(contract);
            _context.SaveChanges();
            return Created(new Uri(Request.RequestUri + "/" + contract.Id), contract);
        }

        //Bajas
        // POST api/Contract/Baja/5
        [HttpPost]
        [Route("api/Contract/Baja/{id}")]
        public IHttpActionResult Baja(int id, ContractDetail contract)
        {
            ContractDetail contractInDB = _context.ContractDetails.FirstOrDefault(d => d.Id == id);
            // contractInDB.EndDate=DateTime.Now;
            contractInDB.EndDate = contract.EndDate;
            contractInDB.Cause = contract.Cause;
            _context.SaveChanges();
            return Ok(contractInDB);
        }

        // PUT api/Contract/5
        [HttpPut]
        public IHttpActionResult Put(int id, [FromBody]ContractDetail contract)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            ContractDetail contractInDB = _context.ContractDetails.FirstOrDefault(d => d.Id == id);
            if (contractInDB == null)
                return NotFound();

            contractInDB.StartDate = contract.StartDate;
            contractInDB.Dedication = contract.Dedication;
            contractInDB.BranchesId = contract.BranchesId;
            contractInDB.DependencyId = contract.DependencyId;
            contractInDB.PositionsId = contract.PositionsId;
            contractInDB.PositionDescription = contract.PositionDescription;
            contractInDB.Linkage = contract.Linkage;
            contractInDB.AI = contract.AI;

            _context.SaveChanges();
            return Ok(contractInDB);
        }

        // DELETE api/Contract/5
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var contractInDB = _context.Contracts.FirstOrDefault(d => d.Id == id);
            if (contractInDB == null)
                return NotFound();
            //_context.Contracts.Remove(contractInDB);
            //_context.SaveChanges();
            return Ok();
        }
    }
}
