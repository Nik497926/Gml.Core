using System;
using System.Collections.Generic;
using Gml.Core.Constants;
using Gml.Core.Helpers.BugTracker;
using Gml.Core.Helpers.Files;
using Gml.Core.Helpers.Launcher;
using Gml.Core.Helpers.Mods;
using Gml.Core.Helpers.Notifications;
using Gml.Core.Helpers.Profiles;
using Gml.Core.Helpers.User;
using Gml.Core.Integrations;
using Gml.Core.Launcher;
using Gml.Core.Services.Storage;
using GmlCore.Interfaces;
using GmlCore.Interfaces.Integrations;
using GmlCore.Interfaces.Launcher;
using GmlCore.Interfaces.Procedures;
using GmlCore.Interfaces.Storage;

namespace Gml;

public class GmlManager : IGmlManager
{
    private readonly IGmlSettings _settings;

    public GmlManager(IGmlSettings settings)
    {
        _settings = settings;
        LauncherInfo = new LauncherInfo(settings);
        Storage = new SqliteStorageService(settings);
        BugTracker = new BugTrackerProcedures(Storage, settings);
        Notifications = new NotificationProcedures(Storage);
        Profiles = new ProfileProcedures(LauncherInfo, Storage, Notifications, this);
        Files = new FileStorageProcedures(LauncherInfo, Storage);
        Mods = new ModsProcedures();
        Integrations = new ServicesIntegrationProcedures(settings, Storage);
        Users = new UserProcedures(settings, Storage, this);
        Launcher = new LauncherProcedures(LauncherInfo, Storage, Files);
        Servers = (IProfileServersProcedures)Profiles;
    }

    public IStorageService Storage { get; }
    public IModsProcedures Mods { get; }
    public ISystemProcedures System => _settings.SystemProcedures;
    public ILauncherInfo LauncherInfo { get; }
    public IBugTrackerProcedures BugTracker { get; }
    public IProfileProcedures Profiles { get; }
    public IProfileServersProcedures Servers { get; }
    public IFileStorageProcedures Files { get; }
    public IServicesIntegrationProcedures Integrations { get; }
    public IUserProcedures Users { get; }
    public ILauncherProcedures Launcher { get; }
    public INotificationProcedures Notifications { get; }

    public void RestoreSettings<T>() where T : IVersionFile
    {
        try
        {
            Profiles.RestoreProfiles().Wait();
            Notifications.Retore().Wait();

            var versionReleases = Storage.GetAsync<Dictionary<string, T?>>(StorageConstants.ActualVersionInfo).Result;

            if (versionReleases is null) return;

            foreach (var item in versionReleases) LauncherInfo.ActualLauncherVersion.Add(item.Key, item.Value);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}
