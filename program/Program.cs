using LLama.Common;
using LLama;
using IoC;
using Autofac;
using Run;
using System.Reflection;

namespace HelloWorld;

class Program
{   
    
    static void Main(string[] args) 
    {   
        Console.WriteLine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        new IoCModule(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json" )).Container().Resolve<IRun>().Run();
    }
}