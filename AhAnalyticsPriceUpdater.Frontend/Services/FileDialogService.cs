using System.IO;
using AhAnalyticsPriceUpdater.Frontend.Interfaces;
using Microsoft.Win32;

namespace AhAnalyticsPriceUpdater.Frontend.Services;

public class FileDialogService : IDialogService
{
    private const string AccountNamePlaceholder    = "ACCOUNTNAME";
    private const string RelativeAccountDirectory  = "_classic_era_\\WTF\\Account";
    private const string RelativeScanDataDirectory = $"{AccountNamePlaceholder}\\SavedVariables";

    public async Task<string> SelectDirectory(string description = "Select a folder")
    {
        var folderDialog = new OpenFolderDialog
        {
            Multiselect = false
        };

        var wowInstallationRoot = string.Empty;

        if (folderDialog.ShowDialog() == true)
            wowInstallationRoot = folderDialog.FolderName;
        //
        // await Task.Run(() =>
        // {
        //     var accountDirectory = Path.Combine(wowInstallationRoot, RelativeAccountDirectory);
        //
        //     var accountNames = Directory.GetDirectories(accountDirectory)
        //                                 .Where(d => !d.Contains("SavedVariables"))
        //                                 .Select(d => d.Split('\\').Last())
        //                                 .ToList();
        //
        //
        // });

        var incompleteRelativePath = Path.Combine(RelativeAccountDirectory, RelativeScanDataDirectory);
        var incompleteFullPath     = Path.Combine(wowInstallationRoot, incompleteRelativePath);

        return incompleteFullPath;
    }

    public async Task<string> SelectWoWInstallationRoot() => await SelectDirectory("Select WoW Installation Root Directory");
}