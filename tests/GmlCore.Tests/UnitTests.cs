using System.Diagnostics;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Utils;
using Gml;
using Gml.Core.Launcher;
using Gml.Core.User;
using Gml.Models;
using GmlCore.Interfaces.Enums;
using GmlCore.Interfaces.Launcher;
using OsType = Gml.Web.Api.Domains.System.OsType;

namespace GmlCore.Tests;

public class Tests
{
    private IGameProfile _testGameProfile = null!;
    private StartupOptions _options;

    private GmlManager GmlManager { get; } = new(new GmlSettings("GamerVIILauncher"));

    [SetUp]
    public async Task Setup()
    {
        await GmlManager.Profiles.RestoreProfiles();


        _options = new StartupOptions
        {
            MinimumRamMb = 4096,
            FullScreen = false,
            ScreenHeight = 600,
            ScreenWidth = 900,
            ServerIp = null,
            ServerPort = 25565,
            MaximumRamMb = 8192
        };
    }

    [Test, Order(0)]
    public void InitializeTest()
    {
        Assert.Multiple(() =>
        {
            Assert.That(GmlManager.LauncherInfo.InstallationDirectory, Is.Not.Empty);
            Assert.That(GmlManager.LauncherInfo.BaseDirectory, Is.Not.Empty);
            Assert.That(GmlManager.LauncherInfo.Name, Is.Not.Empty);
        });
    }

    [Test, Order(1)]
    public async Task CreateProfile()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        Assert.Multiple(() =>
        {
            Assert.That(_testGameProfile, Is.Not.Null);
            Assert.That(_testGameProfile.GameVersion, Is.Not.Empty);
        });
    }

    [Test, Order(2)]
    public async Task AddProfile()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        Assert.Multiple(async () =>
        {
            Assert.That(await GmlManager.Profiles.CanAddProfile("HiTech", "1.20.1"), Is.False);
        });
    }

    [Test, Order(3)]
    public async Task ValidateProfile()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        Assert.Multiple(async () => { Assert.That(await _testGameProfile!.ValidateProfile(), Is.True); });
    }

    [Test, Order(4)]
    public async Task RemoveProfile()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        await _testGameProfile.Remove();
    }

    [Test, Order(5)]
    public async Task ChangeLoaderTypeAndSaveProfiles()
    {
        await GmlManager.Profiles.SaveProfiles();
    }

    [Test, Order(6)]
    public async Task DownloadProfile()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        if (await _testGameProfile.CheckIsFullLoaded(_options) == false)
            await _testGameProfile.DownloadAsync(_options.OsType, _options.OsArch);
    }

    [Test, Order(7)]
    public async Task CheckIsFullLoaded()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        Assert.That(await _testGameProfile.CheckIsFullLoaded(_options), Is.True);
    }


    [Test, Order(8)]
    public async Task InstallForgeClient()
    {
        var forgeClient = await GmlManager.Profiles.GetProfile("Aztex")
                          ?? await GmlManager.Profiles.AddProfile("Aztex", "1.20.1", GameLoader.Forge, string.Empty,
                              string.Empty)
                          ?? throw new Exception("Failed to create profile instance");

        if (await forgeClient.CheckIsFullLoaded(_options) == false)
            await forgeClient.DownloadAsync(_options.OsType, _options.OsArch);

        var process = await forgeClient.CreateProcess(_options, User.Empty);

        var processUtil = new ProcessUtil(process);

        processUtil.OutputReceived += (s, message) => Console.WriteLine(message);
        processUtil.StartWithEvents();
        await processUtil.WaitForExitTaskAsync();

        // Assert.That(await forgeClient.CheckIsFullLoaded(), Is.True);
    }


    [Test, Order(9)]
    public async Task ClientStartup()
    {
        _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
                           ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
                               string.Empty)
                           ?? throw new Exception("Failed to create profile instance");

        var process = await _testGameProfile.CreateProcess(_options, User.Empty);

        var processUtil = new ProcessUtil(process);

        processUtil.OutputReceived += (s, message) => Console.WriteLine(message);
        processUtil.StartWithEvents();
        await processUtil.WaitForExitTaskAsync();
    }



    [Test, Order(999)]
    public async Task CheckInstallationFromOriginalCmlLib()
    {
        var path = new MinecraftPath();
        var launcher = new CMLauncher(path);
        var forge = new MForge(launcher);
        forge.InstallerOutput += (s, e) => Console.WriteLine(e);

        var versionName = await forge.Install("1.7.10");

        var launchOption = new MLaunchOption
        {
            MaximumRamMb = 1024,
            Session = MSession.GetOfflineSession("TaiogStudio"),
        };

        var process = await launcher.CreateProcessAsync(versionName, launchOption);

        process.Start();
        // _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
        //                    ?? await GmlManager.Profiles.AddProfile("HiTech", "1.7.10", GameLoader.Forge, string.Empty,
        //                        string.Empty)
        //                    ?? throw new Exception("Failed to create profile instance");
        //
        // await GmlManager.Profiles.DownloadProfileAsync(_testGameProfile, _options.OsType, _options.OsArch);
        //
        // var myProcess = await _testGameProfile.CreateProcess(_options, User.Empty);
        //
        // Console.WriteLine();



        // process.Start();

        // Console.WriteLine();

        // _testGameProfile = await GmlManager.Profiles.GetProfile("HiTech")
        //                    ?? await GmlManager.Profiles.AddProfile("HiTech", "1.20.1", GameLoader.Vanilla, string.Empty,
        //                        string.Empty)
        //                    ?? throw new Exception("Failed to create profile instance");
        //
        // var process = await _testGameProfile.CreateProcess(new StartupOptions
        // {
        //     MinimumRamMb = 4096,
        //     FullScreen = false,
        //     ScreenHeight = 600,
        //     ScreenWidth = 900,
        //     ServerIp = "",
        //     ServerPort = 25565,
        //     MaximumRamMb = 4096
        // }, new User
        // {
        //     Name = "GamerVII"
        // });
        //
        // var processUtil = new ProcessUtil(process);
        //
        // processUtil.OutputReceived += (s, message) => Console.WriteLine(message);
        // processUtil.StartWithEvents();
        // await processUtil.WaitForExitTaskAsync();
    }


}
