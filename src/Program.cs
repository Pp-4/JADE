using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using JADE.Learning;
using JADE.models;
using JADE.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace JADE;

public partial class Program
{
    public static async Task<int> Main(string[] args)
    {
        Jade program = new();
        using JadeDbContext context = new(program.Config);
        DbWorkflows dbWorkflows = new(program.Config, program.Logger);
        var list = await dbWorkflows.LoadProductsFromJson();
        await dbWorkflows.SaveProductsToDb(list);
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
