namespace Context;

public interface IContext<T>
{
    public Task<T> Init(); 
}


