using System.IO;
using AhAnalyticsPriceUpdater.Frontend.Interfaces;
using Microsoft.Win32;

namespace AhAnalyticsPriceUpdater.Frontend.Services;

public class FileDialogService : IDialogService
{
    private const string  BattleNet                 = "BattleNet";
    private const string  WorldOfWarcraft           = "World of Warcraft";
    public const  string  AccountNamePlaceholder    = "ACCOUNTNAME";
    private const string  RelativeAccountDirectory  = "_classic_era_\\WTF\\Account";
    private const string  RelativeScanDataDirectory = $"{AccountNamePlaceholder}\\SavedVariables";
    private       string? wowInstallationRoot;

    public async Task<string> SelectDirectory(string description = "Select a folder")
    {
        var folderDialog = new OpenFolderDialog
        {
            Multiselect = false
        };

        wowInstallationRoot = string.Empty;

        if (folderDialog.ShowDialog() == true)
            wowInstallationRoot = folderDialog.FolderName;

        if (!wowInstallationRoot.Contains(BattleNet) || !wowInstallationRoot.Contains(WorldOfWarcraft))
            throw new Exception($"Falsches Directory! Da will ich hin: \"..\\{BattleNet}\\{WorldOfWarcraft}");

        var incompleteRelativePath = Path.Combine(RelativeAccountDirectory, RelativeScanDataDirectory);
        var incompleteFullPath     = Path.Combine(wowInstallationRoot, incompleteRelativePath);

        return incompleteFullPath;
    }

    public async Task<string[]> GetAccountNames()
    {
        var accountDirectory = Path.Combine(wowInstallationRoot!, RelativeAccountDirectory);

        var accountNames = Directory.GetDirectories(accountDirectory)
                                    .Where(d => !d.Contains("SavedVariables"))
                                    .Select(d => d.Split('\\').Last())
                                    .ToArray();

        return accountNames;
    }

    public async Task<string> SelectWoWInstallationRoot() => await SelectDirectory("Select WoW Installation Root Directory");
}