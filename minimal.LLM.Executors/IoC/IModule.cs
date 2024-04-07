using Autofac;

namespace IoC;
public interface IModule <T>
{
    IContainer Container();
    T Configuration();
};