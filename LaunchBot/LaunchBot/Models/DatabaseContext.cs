using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace LaunchBot.Models
{
    class DatabaseContext: DbContext
    {
        public DatabaseContext() : base("DatabaseContext") { }

        public DbSet<User> Users { get; set; }
    }
}
