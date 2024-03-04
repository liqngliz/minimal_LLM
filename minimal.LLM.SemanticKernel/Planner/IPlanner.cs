namespace Planner;

public interface IPlanner<T, C>
{
    T CreatePlanAsync(C Inputs);
}
