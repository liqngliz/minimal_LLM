namespace minimal.LLM.Console;

public interface IRouter <T>
{
    public T route(Mode mode, string input);
}

public enum Mode
{
    Interactive,
    Planned
}