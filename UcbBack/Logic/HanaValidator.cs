﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UcbBack.Models;

namespace UcbBack.Logic
{
    public class HanaValidator
    {
        private ApplicationDbContext _context;

        public HanaValidator(ApplicationDbContext context)
        {
            _context = context;
        }
        public string CleanText(string value)
        {
            return _context.Database.SqlQuery<string>("select clean_text('" + value + "') from dummy;").ToList()[0];
        }

        public float JaroWinklerSimilarity(string a, string b)
        {
            return _context.Database.SqlQuery<float>("select jaro_winkler_similarity(clean_text('" + a + "','" + b + "')) from dummy;").ToList()[0];
        }

        //returns a list of strings containing the first 5 similarities
        //  a               =   the value to comapre
        //  colToCompare    =   the colum to compare in the table could accept sql functions
        //  table           =   the name of the table to compare
        //  colId           =   el valor retornado de la busqueda table<Id>
        //  n               =   the probability od similarity
        public List<string> Similarities(string a, string colToCompare,string table,string colId, float n)
        {
            string query = "call SIMILARITIES_TP('" + a + "', " + n.ToString().Replace(",", ".") + ", '" + table + "'," + colToCompare + ",'" + colId + "');";
            return _context.Database.SqlQuery<string>(query).ToList();
        }

    }
}