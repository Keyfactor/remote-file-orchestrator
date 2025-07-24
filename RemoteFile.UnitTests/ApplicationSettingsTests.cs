using Xunit;
using Keyfactor.Extensions.Orchestrator.RemoteFile;

namespace RemoteFile.UnitTests;

public class ApplicationSettingsTests
{
    [Fact]
    public void FileTransferProtocol_WhenPopulatedWithValidValue_ReturnsValue()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "config", "valid", "config.json");
        ApplicationSettings.Initialize(path);
        Assert.Equal(ApplicationSettings.FileTransferProtocolEnum.SCP, ApplicationSettings.FileTransferProtocol);
    }
    
    [Fact]
    public void FileTransferProtocol_WhenAllThreePopulated_DefaultsToBoth()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "config", "file_transfer_protocol_all_three", "config.json");
        ApplicationSettings.Initialize(path);
        Assert.Equal(ApplicationSettings.FileTransferProtocolEnum.Both, ApplicationSettings.FileTransferProtocol);
    }
}
