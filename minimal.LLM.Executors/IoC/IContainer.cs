using Autofac;

namespace Ioc;
public interface IContainer <T>
{
    IContainer Container();
    T Configuration();
};