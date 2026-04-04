namespace PacsApi
{
    public static class GeneralInformation
    {
        public static string BaseDirectory
        {
            get
            {
                var baseDir = @"E:\";

                if (!Directory.Exists(baseDir))
                    throw new DirectoryNotFoundException("E drive not found");

                var appDir = Path.Combine(baseDir, "PacsApi", "Data");

                if (!Directory.Exists(appDir))
                {
                    Directory.CreateDirectory(appDir);
                }

                return appDir;
            }
        }
        public static string DatabaseName = "PacsDB.db";
    }
}
