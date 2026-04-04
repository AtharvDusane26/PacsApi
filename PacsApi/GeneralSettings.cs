namespace PacsApi
{
    public static class GeneralSettings
    {
        private static string _serverName = "Atharv-PC\\SQLEXPRESS";
        private static string _databaseName = "PacsDB";
        private static string _username = "pacsapi";
        private static string _password = "pacs@#$";
        public static int BatchSize = 50;
        public static string ApiToken = "PACSAPI2026TEST";
        public static string BaseDirectory
        {
            get
            {
                var baseDir = @"E:\";
                if (!Directory.Exists(baseDir))
                    baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appDir = Path.Combine(baseDir, "PacsApi", "Data");
                if (!Directory.Exists(appDir))
                    Directory.CreateDirectory(appDir);
                return appDir;
            }
        }
        public static string ConnectionString
        {
            get
            {
                return $"Server={_serverName};Database={_databaseName};User Id={_username};Password={_password};TrustServerCertificate=True;MultipleActiveResultSets=True;";
            }
        }

    }
}
