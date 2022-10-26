using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoboWorkerService;
using RoboWorkerService.Config;
using RoboWorkerService.Interfaces;

//Fluentvalidation
// https://fluentassertions.com/introduction#detecting-test-frameworks

namespace RoboMsTest;

/// <summary>  Baova trida testu, ktera dedi z VerifyBase </summary>
public class BaseTest : VerifyBase
{
    //Arrange
    Dictionary<string, string> inMemorySettings = new Dictionary<string, string>
    {
        { "RoboApp:ActualMarket", "TestMarket" },
        { "RoboApp:RoboDataPath", Environment.CurrentDirectory },
        { "RoboApp:IsDevelopment", "true" },
        //...populate as needed for the test
    };

    public static ServiceCollection Services = new ServiceCollection();
    public static ServiceProvider ServiceProvider;
    private IConfiguration configuration;
    protected VerifySettings VerifySettingsTest;
    protected string VerifyFolder;

    public BaseTest()
    {
        Build();
        Services.AddSingleton<IAppRobo, AppRoboFake>();
        Services.AddSingleton<IConfig, ConfigFake>();
        Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        InitService(Services);
        ServiceProvider = Services.BuildServiceProvider();

        VerifyFolder = GeCompareDataFolderPath() + @"\Verify\";
        Directory.CreateDirectory(VerifyFolder);
        VerifySettingsTest = new VerifySettings();
        VerifySettingsTest.UseDirectory(VerifyFolder);
    }


    public virtual void InitService(ServiceCollection service)
    {
    }

    public void Build()
    {
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        Services.AddSingleton(typeof(IConfiguration), configuration);
    }

    public void RegisterAppServices()
    {
        Services.AddRoboServices();
        Services.AddJsonService();
    }

    public string GeCompareDataFolderPath()
    {
        return new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory?.Parent?.Parent?.Parent
            ?.FullName ?? Environment.CurrentDirectory;
    }
}