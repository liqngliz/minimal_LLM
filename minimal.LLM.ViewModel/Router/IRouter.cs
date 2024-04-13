namespace Router;

public interface IRouter <T>
{
    public T route(T routingInput);
}

public enum Mode
{
    Interactive,
    FunctionPlan,
    StepsPlan,
    Result
}