Minimal C# wrapper for LLM's such as LLAMA 2

Compatible with any LLM in GGUF format.

Compiles to any x86, x64, ARM architecture

Runs as console app

Configurable GPU offloading (Max 20)

For JSON configuration (config.json)

{
    "ContextSize": <Size of Context number of tokens>,
    "Seed": <Seed randomized generation>,
    "GpuLayerCount": <Max 20, GPU offloading, reduce to reduce ram usuage>,
    "MaxTokens": <Max tokens per prompt>,
    "Temperature": <Temperature affect randomization>,
    "RepeatPenalty": <Repeat penalty, affects randomization>,
    "Model": <GGUF model file location>,
    "Prompt": <prompt.txt file location to initiate conversation>
}

To run program and tests, openhermes-2.5-mistral-7b.Q4_K_M or equivalent is required, add model to bin folder.

GGUF Files:
https://huggingface.co/TheBloke/OpenHermes-2.5-Mistral-7B-GGUF/tree/main
OpenHermes Project Page:
https://huggingface.co/teknium/OpenHermes-2.5-Mistral-7B