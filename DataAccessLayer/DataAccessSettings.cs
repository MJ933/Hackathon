using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class DataAccessSettings
    {
        private static string? _connectionString;

        public static void Initialize(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {

                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
            }
        }

        public static NpgsqlDataSource CreateDataSource()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string has not been initialized. Call Initialize first.");
            return NpgsqlDataSource.Create(_connectionString);
        }



    }
}
