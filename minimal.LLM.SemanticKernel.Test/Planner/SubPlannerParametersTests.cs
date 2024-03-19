using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel.Plugins;


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
    }

    [Theory]
    [InlineData("MathPlugin", "Sqrt", 1)]
    [InlineData("MathPlugin", "Add", 2)]
    [InlineData("MathPlugin", "Subtract", 2)]
    [InlineData("MathPlugin", "Multiply", 2)]
    public async void TestsForAskingUserReplies(string plugin, string function, int expectedReplies)
    {
        var func = _kernel.Plugins.GetFunction(plugin, function);
        sut = new Planner.Parameters.SubPlannerParameter();
        var plans = await sut.Plan(func);
        Assert.Equal(expectedReplies, plans.Count);
    }
}