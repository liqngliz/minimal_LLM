using Autofac;
using Configuration;
using Context;
using IoC;
using Run;

public class ContextTest 
{
    readonly IContext<LlamaInstance> _sut;

    public ContextTest () 
    {
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<IContext<LlamaInstance>>();
    }
}