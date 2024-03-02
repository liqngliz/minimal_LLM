using Autofac;

namespace Factory;

public interface IFactory <T>
{   
    T Make(Type Type);
}
