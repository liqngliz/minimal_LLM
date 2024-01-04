using Prompts;
using System.IO;
using Newtonsoft.Json;
using Configuration;
using llm;
using Context;

public class PromptConstructLatin : IPromptConstruct<Task<List<string>>>
{   
    readonly PromptParams _promptParams;
    readonly Config _config;
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance> _llamaSharpLlm;
    readonly List<string> _chunks;

    public PromptConstructLatin(PromptParams promptParams, Config config, Illm<IAsyncEnumerable<string>, string, LlamaInstance> llamaSharpLlm)
    {
        _promptParams = promptParams;
        _config = config;
        _chunks = new List<string>();
        _llamaSharpLlm = llamaSharpLlm;

        string[] delimiterChars = new string[]{ " ", "\r\n", "\r", "\n","\t" };

        var text = string.Join(" ",File.ReadAllText(promptParams.ContextFile).Split(delimiterChars, StringSplitOptions.TrimEntries));
        
        while(text.Length > _promptParams.ChunkSize)
        {   
            var chunk = text.Substring(0, _promptParams.ChunkSize);
            _chunks.Add(chunk);
            text = text.Substring(_promptParams.ShiftSize);
        }
        if(text.Length > 0) _chunks.Add(text);
    }

    public async Task<List<string>> Construct(string prompt)
    {
        var res = new List<string>();
        if(_chunks.Count <= _promptParams.Segments) return _chunks;
        res = _chunks;
        int length = res.Count;

        string temp = res[0];
        var tempList = new List<string>();
        
        var initPrompt = "Transcript of a dialog, where the User interacts with an assistant named TScript.\nUser: Hello, TScript.\nPypy:Hello. How may I help you today with you questions?\n";
        
        await foreach (var text in _llamaSharpLlm.Infer(initPrompt)) 
        {   
                Console.Write(text);
        }

        for (int i = 0; i < length; i++)
        {
            for (int j = i+1; j < length; j++)
            {   
                string iContext = res[i];
                string jContext = res[j];
                bool iBetter = false;
                string question = $"Answer only with yes or no and nothing else. Is text \"i\" more relevant than text \"j\" to the question \"{prompt}\"." 
                + $"\"i\":\"{iContext}\", \"j\":\"{jContext}\"";
                Console.WriteLine(question);

                string Answer = "";
                await foreach (var text in _llamaSharpLlm.Infer(question)) 
                {   
                    Console.WriteLine(text);
                    Answer = Answer + text;
                }

                if(Answer.Contains("yes", StringComparison.InvariantCultureIgnoreCase)) iBetter = true;

                if (iBetter)
                {
                    temp = iContext;

                    res[i] = res[j];

                    res[j] = temp;
                }
                
            }
        }
        res.Reverse();
        return res.Take(_promptParams.Segments).ToList();
    }
}