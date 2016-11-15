using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThinkingHome.Migrator
{
    public class Class1
    {
        public void Run(string cstring)
        {
            using (var conn = new Npgsql.NpgsqlConnection(cstring))
        }
    }
}