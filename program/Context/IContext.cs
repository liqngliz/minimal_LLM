namespace Context;
//renamed
public interface IContext<T>
{
    public Task<T> Init(); 
}


