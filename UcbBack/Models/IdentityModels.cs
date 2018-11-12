﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using UcbBack.Models.Auth;
using UcbBack.Models.Dist;

namespace UcbBack.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<People> Person { get; set; }
        public DbSet<Branches> Branch { get; set; }
        public DbSet<Positions> Position { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<OrganizationalUnit> OrganizationalUnits { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<PerformanceArea> PerformanceAreas { get; set; }
        public DbSet<Contract> Contracts  { get; set; }
        public DbSet<ContractDetail> ContractDetails  { get; set; }
        public DbSet<Dist_Academic> DistAcademics  { get; set; }
        public DbSet<Dist_Payroll> DistPayrolls  { get; set; }
        public DbSet<Dist_Discounts> DistDiscountses  { get; set; }
        public DbSet<Dist_Pregrado> DistPregrados  { get; set; }
        public DbSet<Dist_Posgrado> DistPosgrados  { get; set; }
        public DbSet<Dist_OR> DistOrs  { get; set; }
        public DbSet<CuentaContable> CuentaContables  { get; set; }
        public DbSet<GrupoContable> GrupoContables  { get; set; }
        public DbSet<Gestion> Gestions  { get; set; }
        public DbSet<TipoEmpleadoDist> TipoEmpleadoDists  { get; set; }
        public DbSet<Dist_File> FileDbs  { get; set; }
        public DbSet<Dist_LogErrores> DistLogErroreses  { get; set; }
        public DbSet<Dist_Process> DistProcesses  { get; set; }
        public DbSet<Dist_FileType> DistFileTypes  { get; set; }
        public DbSet<Module> Modules  { get; set; }
        public DbSet<Resource> Resources  { get; set; }
        public DbSet<AccessLogs> AccessLogses  { get; set; }
        public DbSet<B1SDKLog> SdkErrorLogs  { get; set; }
        public DbSet<TableOfTables> TableOfTableses { get; set; }
        public DbSet<TempAlta> TempAltas { get; set; }
        

        //auth models
        public DbSet<Access> Accesses { get; set; }
        public DbSet<Rol> Rols { get; set; }
        public DbSet<RolhasAccess> RolshaAccesses { get; set; }
        public DbSet<CustomUser> CustomUsers { get; set; }

        
        

        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {     
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("ADMNALRRHH");
           // modelBuilder.Ignore<People>();
        }
    }
}