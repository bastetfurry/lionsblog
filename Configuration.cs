namespace LionsBlog;

public class Configuration
{
    public string DefaultUser { get; set; }
    public string DefaultPassword { get; set; }
    public string DefaultScreenname { get; set; }
    public string DefaultEMail { get; set; }
    public string DatabaseFilename { get; set; }
    public string TokenSecret { get; set; }
    public int PostsPerPage { get; set; }
    public string ImageDirectory { get; set; }
    public bool EnforceHTTPS { get; set; }
    public bool EnableJSFeatures { get; set; }
}

public class ConfigurationProvider
{
    private static Configuration _configuration;

    public static void SetConfiguration(IConfiguration configuration)
    {
        _configuration = configuration.GetSection("LionsBlog").Get<Configuration>();
    }

    public static Configuration GetConfiguration()
    {
        return _configuration;
    }

}