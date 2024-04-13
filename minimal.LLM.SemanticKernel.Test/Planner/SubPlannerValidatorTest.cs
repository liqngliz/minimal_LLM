using Autofac;
using Microsoft.SemanticKernel;
using Plugins;
using Planner;
using Planner.Validators;

namespace PlannerTests;

public class SubPlannerValidatorTest
{   
    private IPlanner<Task<Validation>, KernelParamValidationPlan> sut;
    readonly IKernelBuilder _builder;
    readonly Kernel _kernel;
    public SubPlannerValidatorTest()
    {
        _builder = Kernel.CreateBuilder();
        _builder.Plugins.AddFromType<MathPlugin>();
        _kernel = _builder.Build();
        sut = new SubPlannerValidator();
    }
    
    [Theory]
    [InlineData("MathPlugin", "Sqrt", "number1", "1", "1", true)]
    [InlineData("MathPlugin", "Add", "number1", "56", "56", true)]
    [InlineData("MathPlugin", "Add", "number2", "36", "36", true)]
    [InlineData("MathPlugin", "Add", "number2", "hey some value", "<|im_start|>Bob\nYour input could not be parsed as System.Double and resulted in the following error 'Input string was not in a correct format.'<|im_end|>Prohibere", false)]
    [InlineData("MathPlugin", "Subtract", "number1", "-1", "-1", true)]
    [InlineData("MathPlugin", "Multiply", "number2", "2.786", "2.786", true)]
    public async void should_parse(string plugin, string function, string param, string input, string expected, bool validity)
    {
        var func = _kernel.Plugins.GetFunction(plugin, function);
        var parameter = func.Metadata.Parameters.Where(x => x.Name == param).Single();
        
        var result = await sut.Plan(new KernelParamValidationPlan(parameter, input));
        Assert.Equal(result.Valid, validity);
        Assert.Equal(expected, result.Value.ToString());
    }
}