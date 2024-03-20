using Microsoft.SemanticKernel;
using minimal.LLM.SemanticKernel.Plugins;
using Planner;
using Planner.Parameters;
using Planner.StepPlanner;
using Planner.Validators;

namespace PlannerTests;

public class StepPlannerTest
{
    private IPlanner<Task<StepResult>, string> sut;
    readonly IKernelBuilder _builder;
    readonly Kernel _kernel;
    public StepPlannerTest()
    {
        _builder = Kernel.CreateBuilder();
        _builder.Plugins.AddFromType<MathPlugin>();
        _builder.Plugins.AddFromType<FilePlugin>();
        _kernel = _builder.Build();
        
    }

    [Theory]
    [InlineData("MathPlugin", "Sqrt", new string[]{"What is the root of 9","9"}, 3)]
    [InlineData("MathPlugin", "Add", new string[]{"What is 9 plus 10","9","10"}, 19)]
    [InlineData("MathPlugin", "Multiply", new string[]{"What is 9 * 10","9","bad input","10"}, 90)]

    public async void should_invoke_semantic_function(string plugin, string function, string[] sequence, object expected)
    {
        var func = _kernel.Plugins.GetFunction(plugin, function);
        sut = new StepPlanner(new SubPlannerParameter(), new SubPlannerValidator(), func, _kernel);
        for(var i = 0; i < sequence.Length; i++)
        {
            var res = await sut.Plan(sequence[i]);
            
            if(i == sequence.Length) 
            Assert.Equal(expected, res.FunctionResult);
        }
    }
}