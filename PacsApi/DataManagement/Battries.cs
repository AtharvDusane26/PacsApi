using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace PacsApi.DataManagement
{
    public static class Battries
    {
        public static async Task Init()
        {
            var builder = new SqlConnectionStringBuilder(GeneralSettings.ConnectionString);

            string server = builder.DataSource;
            string database = builder.InitialCatalog;
            string user = builder.UserID;
            string password = builder.Password;

            // 🔹 Admin connection (Windows Auth)
            var adminConnection =
                $"Server={server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

            using var connection = new SqlConnection(adminConnection);
            await connection.OpenAsync();

            // 🔹 1. Create Login (if not exists)
            var createLogin = $@"
            IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '{user}')
            BEGIN
                CREATE LOGIN [{user}] WITH PASSWORD = '{password}';
            END";

            await new SqlCommand(createLogin, connection).ExecuteNonQueryAsync();

            // 🔹 2. Create Database (if not exists)
            var createDb = $@"
            IF DB_ID('{database}') IS NULL
            BEGIN
                CREATE DATABASE [{database}];
            END";

            await new SqlCommand(createDb, connection).ExecuteNonQueryAsync();

            // 🔹 3. Create User inside DB
            var dbConnectionString =
                $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;";

            using var dbConnection = new SqlConnection(dbConnectionString);
            await dbConnection.OpenAsync();

            var createUser = $@"
            IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '{user}')
            BEGIN
                CREATE USER [{user}] FOR LOGIN [{user}];
                ALTER ROLE db_owner ADD MEMBER [{user}];
            END";

            await new SqlCommand(createUser, dbConnection).ExecuteNonQueryAsync();
        }
    }
}
