namespace PacsApi
{
    public static class GeneralSettings
    {
        public static int BatchSize = 50;

        public static string ApiToken = "PACSAPI2026TEST";

        public static string BaseDirectory
        {
            get
            {
                var baseDir = @"E:\";

                if (!Directory.Exists(baseDir))
                    baseDir = Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData);

                var appDir = Path.Combine(baseDir, "PacsApi", "Data");

                if (!Directory.Exists(appDir))
                    Directory.CreateDirectory(appDir);

                return appDir;
            }
        }

        public static string ConnectionString { get; private set; } = string.Empty;

        public static void Initialize(IConfiguration configuration)
        {
            var serverName = configuration["Database:Server"];
            var databaseName = configuration["Database:DatabaseName"];
            var username = configuration["Database:Username"];
            var password = configuration["Database:Password"];

            var trustServerCertificate =
                configuration.GetValue<bool>("Database:TrustServerCertificate");

            var multipleActiveResultSets =
                configuration.GetValue<bool>("Database:MultipleActiveResultSets");

            ConnectionString =
                $"Server={serverName};" +
                $"Database={databaseName};" +
                $"User Id={username};" +
                $"Password={password};" +
                $"TrustServerCertificate={trustServerCertificate};" +
                $"MultipleActiveResultSets={multipleActiveResultSets};";
        }
    }
}