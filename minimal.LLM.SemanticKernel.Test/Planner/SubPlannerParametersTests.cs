using Microsoft.SemanticKernel;
using Plugins;


namespace PlannerTests;

public class SubPlannerParametersTest
{   
    private Planner.IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction> sut;
    readonly IKernelBuilder _builder;
    readonly Kernel _kernel;
    public SubPlannerParametersTest()
    {
        _builder = Kernel.CreateBuilder();
        _builder.Plugins.AddFromType<MathPlugin>();
        _kernel = _builder.Build();
        sut = new Planner.Parameters.SubPlannerParameter();
    }

    [Theory]
    [InlineData("MathPlugin", "Sqrt", 1)]
    [InlineData("MathPlugin", "Add", 2)]
    [InlineData("MathPlugin", "Subtract", 2)]
    [InlineData("MathPlugin", "Multiply", 2)]
    public async void should_get_user_replies(string plugin, string function, int expectedReplies)
    {
        var func = _kernel.Plugins.GetFunction(plugin, function);
        
        var plans = await sut.Plan(func);
        Assert.Equal(expectedReplies, plans.Count);
    }
}