namespace minimal.LLM.Console;

public interface IRouter <T>
{
    public T route(InputMode inputMode, string input);
}

public enum InputMode
{
    Interactive,
    Planned
}