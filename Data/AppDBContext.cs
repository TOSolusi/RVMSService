using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RVMSService.Models;
using System.Collections.Generic;

namespace RVMSService.Data
{
    public class AppDBContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<DestinationModel> Destinations { get; set; }
        public DbSet<VisitModel> Visits { get; set; }
        public DbSet<VisitorModel> Visitors { get; set; }
        public DbSet<VisitTypeModel> VisitTypes { get; set; }
        public DbSet<QrCodeModel> QrCodes { get; set; }
        public DbSet<GateModel> Gates { get; set; }


        public AppDBContext(DbContextOptions options) : base(options)
        {
           

        }
    }


}
        