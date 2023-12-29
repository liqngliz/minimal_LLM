using LLama.Common;
using LLama;
using Configuration;
using LLama.Native;

namespace Context;

public record LlamaInstance(ModelParams ModelParams, InferenceParams InferenceParams, string Prompt);

public class LlamaSharpContext : IContext<LlamaInstance>
{   
    readonly Config _config;
    public LlamaSharpContext(Config config)
    {
        _config = config;
    }

    public async Task<LlamaInstance> Init()
    {   

        string modelPath = _config.Model; 
        
        
        string prompt = File.ReadAllText(_config.Prompt);

    
        var parameters = new ModelParams(modelPath)
        {   
            ContextSize = _config.ContextSize,
            Seed = _config.Seed,
            GpuLayerCount = _config.GpuLayerCount
        };

        var inferenceParams = new InferenceParams() 
            { 
                Temperature = _config.Temperature, 
                RepeatPenalty = _config.RepeatPenalty, 
                AntiPrompts = new List<string> { "User:" }, 
                MaxTokens = _config.MaxTokens 
            };
        
        return  new LlamaInstance(parameters, inferenceParams, prompt);
    }
}