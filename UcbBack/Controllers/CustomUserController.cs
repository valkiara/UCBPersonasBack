﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using UcbBack.Logic;
using UcbBack.Models;
using UcbBack.Models.Auth;

namespace UcbBack.Controllers
{
    public class CustomUserController : ApiController
    {
        private ApplicationDbContext _context;
        private ValidateToken validator;
        private ADClass activeDirectory;

        public CustomUserController()
        {
            _context = new ApplicationDbContext();
            validator = new ValidateToken();
            activeDirectory = new ADClass();
        }

        // GET api/user
        [Route("api/user/")]
        public IHttpActionResult Get()
        {
            var userlist = _context.CustomUsers.Include(x=>x.People)
                .ToList().
                Select(x => new
                {
                    x.Id,
                    x.UserPrincipalName,
                    person = x.People.GetFullName(),
                    x.PeopleId
                });
            return Ok(userlist);

        }

        // GET api/user/5
        [Route("api/user/{id}")]
        public IHttpActionResult Get(int id)
        {
            CustomUser userInDB = null;

            userInDB = _context.CustomUsers.Include(x=>x.People).FirstOrDefault(d => d.Id == id);

            if (userInDB == null)
                return NotFound();
            dynamic respose = new JObject();
            respose.Id = userInDB.Id;
            respose.UserPrincipalName = userInDB.UserPrincipalName;
            respose.PeopleId = userInDB.People.Id;
            respose.Name = userInDB.People.GetFullName();
            return Ok(respose);
        }

        // POST: /api/user/
        [HttpPost]
        [Route("api/user/")]
        public IHttpActionResult Register([FromBody]CustomUser user)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            user.Id = _context.Database.SqlQuery<int>("SELECT ADMNALRRHH.\"rrhh_User_sqs\".nextval FROM DUMMY;").ToList()[0];

            user.Token = validator.getToken(user);
            user.TokenCreatedAt = DateTime.Now;
            user.RefreshToken = validator.getRefreshToken(user);
            user.RefreshTokenCreatedAt = DateTime.Now;
            _context.CustomUsers.Add(user);
            _context.SaveChanges();

            dynamic respose = new JObject();
            respose.Id = user.Id;
            respose.UserPrincipalName = user.UserPrincipalName;
            respose.Token = user.Token;
            respose.RefreshToken = user.RefreshToken;

            return Created(new Uri(Request.RequestUri + "/" + respose.Id), respose);
        }

        // GET api/user
        [HttpPut]
        [Route("api/user/{id}")]
        public IHttpActionResult Put(int id, CustomUser user)
        {
            var userInDb = _context.CustomUsers.FirstOrDefault(x => x.Id == id);
            userInDb.UserPrincipalName = user.UserPrincipalName;
            _context.SaveChanges();
            return Ok(userInDb);

        }

        // DELETE api/user/5
        [HttpPost]
        [Route("api/user/ChangeStatus")]
        public IHttpActionResult ChangeStatus(int id)
        {
            var userInDB = _context.CustomUsers.FirstOrDefault(d => d.Id == id);
            if (userInDB == null)
                return NotFound();

            _context.CustomUsers.Remove(userInDB);
            _context.SaveChanges();
            return Ok();
        }
    }
}
