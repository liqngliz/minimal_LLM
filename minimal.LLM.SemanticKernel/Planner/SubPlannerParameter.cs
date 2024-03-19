using Microsoft.SemanticKernel;


namespace Planner.Parameters;

public class SubPlannerParameter : IPlanner<Task<Dictionary<KernelParameterMetadata,string>>, KernelFunction>
{   
    string _query;

    public SubPlannerParameter(string query = null)
    {   
        _query = string.IsNullOrEmpty(query) ? query.ToReplyQuery() : query;
    }

    public async Task<Dictionary<KernelParameterMetadata,string>> Plan(KernelFunction Inputs)
    {    
        string functionName = Inputs.Name;
        string functionDescription = Inputs.Description;

        var parameterMetadata = Inputs.Metadata.Parameters.ToList();

        var output = new Dictionary<KernelParameterMetadata, string>();
        
        parameterMetadata.ForEach(x => {
            string name = x.Name;
            string desc = x.Description;
            string type = x.ParameterType.Name;
        
            string query = _query.Replace("{functionName}",functionName)
                .Replace("{functionDescription}",functionDescription)
                .Replace("{parameter}",name)
                .Replace("{description}", desc)
                .Replace("{type}", type);

            output.Add(x, query);
        });
        
        return output;
    }
}

public static class SubPlannerParameterExtension 
{
    public static string ToReplyQuery(this string input) => "<|im_start|>Bob\nPlease give me the value of '{parameter}', which is of type '{type}' to be used as '{description}' in a function '{functionName}' that '{functionDescription}'<|im_end|>Prohibere";

}