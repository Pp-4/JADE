using System.Threading.Tasks;

namespace JADE;

public partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        Jade program = new();
        await program.Start();
        return 0;
    }

    // public void test()
    // {
    // var webAppBuilder = WebApplication.CreateBuilder();
    // webAppBuilder.Services.AddTickerQ(optional =>
    // {
    // optional.AddOperationalStore(database =>
    // {
    // database.UseApplicationDbContext<JADEDbContext>(ConfigurationType.UseModelCustomizer);
    // });
    // optional.AddDashboard(dash =>
    // {
    // dash.SetBasePath("/dashboard");
    // });
    // });
    // webAppBuilder.Services.AddDbContext<JADEDbContext>();
    // var webApp = webAppBuilder.Build();
    // webApp.UseTickerQ();
    // webApp.Run();
    // }
}
