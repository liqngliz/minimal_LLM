using LLama.Common;
using LLama;
using IoC;
using Autofac;
using Run;

namespace HelloWorld;

class Program
{
    static void Main(string[] args) => new IoCModule("config.json").Container().Resolve<IRun>().Run();
}