using Configuration;
using Ioc;

public sealed class IoCSingleton
{
    private static readonly IoCSingleton instance = new IoCSingleton();
    private static readonly IContainer<Config> ioCModule = new IocContainer("config.json");

    static IoCSingleton()
    {
    }

    private IoCSingleton()
    {
    }

    public static IContainer<Config> Module
    {
        get
        {
            return ioCModule;
        }
    }
}
