using Configuration;
using IoC;

public sealed class IoCSingleton
{
    private static readonly IoCSingleton instance = new IoCSingleton();
    private static readonly IModule<Config> ioCModule = new IoCModule("config.json");

    static IoCSingleton()
    {
    }

    private IoCSingleton()
    {
    }

    public static IModule<Config> Module
    {
        get
        {
            return ioCModule;
        }
    }
}
