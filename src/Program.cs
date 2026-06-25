using System.Threading.Tasks;
using JADE.Learning;
using JADE.models;

namespace JADE;

public partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        Jade program = new();
        using JadeDbContext context = new(program.Config);

        Learning.Product pizza = new()
        {
            Name = "Pizza Vegg",
            Price = 12,
        };
        Learning.Product meatzza = new()
        {
            Name = "Pizza Meat",
            Price = 13,
        };
        context.Add(pizza);
        context.Add(meatzza);
        context.SaveChanges();
        //await program.Start();
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
