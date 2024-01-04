using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

namespace Configuration;

public record Config(uint ContextSize, uint Seed , int GpuLayerCount, int MaxTokens, float Temperature, float RepeatPenalty, Location Model ,Location Prompt); 

public record Location(string locationValue)
{   
    public static implicit operator string(Location location) => location.locationValue;

    public static implicit operator Location(string location) {
        string assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        string assemblyParent = new DirectoryInfo(assemblyDir).Parent.FullName;

        string pathRelative = location switch
        {
            string a when a.IndexOf("../") == 0 => Path.Combine(assemblyParent, location.Replace("../", "")),
            string a when a.IndexOf("..\\") == 0 => Path.Combine(assemblyParent, location.Replace("..\\", "")),
            string a when a.IndexOf("./") == 0 => Path.Combine(assemblyDir, location.Replace("./", "")),
            string a when a.IndexOf(".\\") == 0 => Path.Combine(assemblyDir, location.Replace(".\\", "")),
            _=> Path.Combine(assemblyDir, location)
        };

        return new Location(pathRelative);       
    } 
}

public record PromptParams(Location ContextFile, int ShiftSize, int Segments, int ChunkSize);

