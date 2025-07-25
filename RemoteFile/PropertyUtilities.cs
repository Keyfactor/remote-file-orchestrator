using System;

namespace Keyfactor.Extensions.Orchestrator.RemoteFile;

public static class PropertyUtilities
{
    public static bool TryEnumParse<T>(string value, out bool isFlagCombination, out T result) where T : struct, Enum
    {
        isFlagCombination = false;
        result = default(T);
        
        // First, do the normal TryParse
        if (!Enum.TryParse<T>(value, out result))
        {
            return false;
        }
        
        // Check if the enum has the Flags attribute
        bool hasFlags = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
        
        // If it doesn't have Flags attribute but the input contains commas,
        // this might be unintended flag parsing
        if (!hasFlags && value.Contains(','))
        {
            // Check if the parsed result corresponds to a defined enum value
            if (!Enum.IsDefined(typeof(T), result))
            {
                isFlagCombination = true;
                return true;
            }
        }
        
        return true;
    }
}
