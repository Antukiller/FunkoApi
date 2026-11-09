using DotNet.Testcontainers.Builders;

namespace FunkoApiV2Test.Integration;

internal static class DockerTestPrerequisite {
    public static async Task StartContainerOrIgnoreAsync(Func<Task> start) {
        try {
            await start();
        }
        catch (DockerUnavailableException exception) {
            Assert.Ignore($"Integration tests require a running Docker daemon: {exception.Message}");
        }
    }
}
