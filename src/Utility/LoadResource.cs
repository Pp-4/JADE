using System.Reflection;
using System.IO;

namespace JADE;

public partial class Program
{
    private static Assembly[] AllAssembliesOfCurrentAppDomain => System.AppDomain.CurrentDomain.GetAssemblies();
    internal static string LoadResource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        //list all resources in current assembly
        //Assembly.GetExecutingAssembly().GetManifestResourceNames();
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream is not null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        else throw new FileNotFoundException($"{name} resource not found!");
    }
}