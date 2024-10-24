using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gml.Models.Launcher;
using Gml.Models.Storage;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Storage;

namespace Gml.Core.Launcher;

public class LauncherInfo : ILauncherInfo
{
    public LauncherInfo(IGmlSettings settings)
    {
        Settings = settings;
    }

    public string Name => Settings.Name;
    public string BaseDirectory => Settings.BaseDirectory;
    public string InstallationDirectory => Settings.InstallationDirectory;
    public IGmlSettings Settings { get; }

    public IStorageSettings StorageSettings { get; set; } = new StorageSettings();
    public Dictionary<string, IVersionFile?> ActualLauncherVersion { get; set; } = new();

    public void UpdateSettings(StorageType storageType,
        string storageHost,
        string storageLogin,
        string storagePassword,
        TextureProtocol textureProtocol)
    {
        StorageSettings.StoragePassword = storagePassword;
        StorageSettings.StorageLogin = storageLogin;
        StorageSettings.StorageType = storageType;
        StorageSettings.StorageHost = storageHost;
        StorageSettings.TextureProtocol = textureProtocol;
    }

    public Task<IEnumerable<ILauncherBuild>> GetBuilds()
    {
        var versionsPath = Path.Combine(InstallationDirectory, "LauncherBuilds");

        var directoryInfo = new DirectoryInfo(versionsPath);

        var builds = directoryInfo.GetDirectories();

        var versions = builds.Select(c => new LauncherBuild
        {
            Name = c.Name,
            Path = c.FullName,
            DateTime = c.CreationTime
        });

        return Task.FromResult(versions.OfType<ILauncherBuild>());
    }

    public async Task<ILauncherBuild?> GetBuild(string name)
    {
        return (await GetBuilds()).FirstOrDefault(c => c.Name == name);
    }
}
