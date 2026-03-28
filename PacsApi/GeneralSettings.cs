namespace PacsApi
{
    public static class GeneralSettings
    {
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
        public static string DatabaseName = "PacsDB.db";
        public static int BatchSize = 50;
    }
}
