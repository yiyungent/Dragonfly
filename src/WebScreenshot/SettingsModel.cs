namespace WebScreenshot
{
    public class SettingsModel
    {
        public long CacheMinutes { get; set; } = 60;

        public string ChromeDriverDirectory { get; set; } = "/app/tools/selenium/";

        public string CacheModel { get; set; }
    }
}
