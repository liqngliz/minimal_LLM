namespace Prompts;

public interface IPromptConstruct <T>
{
     T Construct(string prompt);
}