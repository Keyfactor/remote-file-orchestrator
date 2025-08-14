using Xunit;
using Keyfactor.Extensions.Orchestrator.RemoteFile;

namespace RemoteFile.UnitTests;

public class ApplicationSettingsTests
{
    [Fact]
    public void FileTransferProtocol_WhenPopulatedWithValidValue_ReturnsValue()
    {
        Assert.Equal(ApplicationSettings.FileTransferProtocolEnum.SCP, ApplicationSettings.FileTransferProtocol);
    }
    
    [Fact]
    public void FileTransferProtocol_WhenAllThreePopulated_DefaultsToBoth()
    {
        Assert.Equal(ApplicationSettings.FileTransferProtocolEnum.Both, ApplicationSettings.FileTransferProtocol);
    }
}
