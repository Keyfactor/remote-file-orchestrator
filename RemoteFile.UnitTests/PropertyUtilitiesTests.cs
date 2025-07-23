using Keyfactor.Extensions.Orchestrator.RemoteFile;
using Xunit;

namespace RemoteFile.UnitTests;

public class PropertyUtilitiesTests
{
    [Theory]
    [InlineData("SCP", ApplicationSettings.FileTransferProtocolEnum.SCP)]
    [InlineData("SFTP", ApplicationSettings.FileTransferProtocolEnum.SFTP)]
    [InlineData("Both", ApplicationSettings.FileTransferProtocolEnum.Both)]
    public void TryEnumParse_WhenProvidedAValidEnumString_MapsToExpectedEnumValue(string input,
        ApplicationSettings.FileTransferProtocolEnum expected)
    {
        var isValid = PropertyUtilities.TryEnumParse(input, out var isFlagCombination,
            out ApplicationSettings.FileTransferProtocolEnum result);
        
        Assert.True(isValid);
        Assert.Equal(expected, result);
        Assert.False(isFlagCombination);
    }

    [Fact]
    public void TryEnumParse_WhenProvidedAFlagCombination_SetsIsFlagCombination()
    {
        var input = "SCP,SFTP,Both";
        
        var isValid = PropertyUtilities.TryEnumParse(input, out var isFlagCombination,
            out ApplicationSettings.FileTransferProtocolEnum result);
        
        Assert.True(isValid);
        Assert.Equal((ApplicationSettings.FileTransferProtocolEnum) 3, result);
        Assert.True(isFlagCombination);
    }
    
    [Fact]
    public void TryEnumParse_WhenProvidedAnInvalidMapping_MarksIsValidAsFalse()
    {
        var input = "randomstring";
        
        var isValid = PropertyUtilities.TryEnumParse(input, out var isFlagCombination,
            out ApplicationSettings.FileTransferProtocolEnum result);
        
        Assert.False(isValid);
        Assert.Equal(ApplicationSettings.FileTransferProtocolEnum.SCP, result);
        Assert.False(isFlagCombination);
    }
}
