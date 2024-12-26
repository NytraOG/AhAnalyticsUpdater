using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using AhAnalyticsPriceUpdater.Frontend.Interfaces;
using AhAnalyticsPriceUpdater.Frontend.Services;
using AhAnalyticsPriceUpdater.Services;

namespace AhAnalyticsPriceUpdater.Frontend.Models;

public class UpdateProcessViewModel : INotifyPropertyChanged
{
    private readonly IDialogService fileDialogService;
    private readonly SpreadsheetService spreadsheetService;
    private ObservableCollection<string> accountnamesFromInstallationDirectory = new();
    private string? installationRootWorldOfWarcraft;
    private string? selectedAccount;
    private double progessbarValue;

    public UpdateProcessViewModel(SpreadsheetService spreadsheetService, IDialogService fileDialogService)
    {
        ProgessbarValue = 0;
        
        this.fileDialogService = fileDialogService;
        this.spreadsheetService = spreadsheetService;

        StartUpdatePricesProcess = new AsyncRelayCommand(StartUpdatePrices);
        OpenSpreadsheetProcess = new AsyncRelayCommand(OpenSpreadsheet);
        ExecuteFilePickerProcess = new AsyncRelayCommand(ExecuteFilePicker);

        spreadsheetService.ScanningProgressed += SpreadsheetServiceOnScanningProgressed;
    }

    public ICommand StartUpdatePricesProcess { get; }
    public bool StartUpdatePricesInProgress { get; set; }
    public ICommand OpenSpreadsheetProcess { get; }
    public bool OpenSpreadsheetInProgress { get; set; }
    public ICommand ExecuteFilePickerProcess { get; }
    public bool OpenFilePickerInProgress { get; set; }

    public double ProgessbarValue
    {
        get => progessbarValue;
        set => SetField(ref progessbarValue, value);
    }

    public string? InstallationRootWorldOfWarcraft
    {
        get => installationRootWorldOfWarcraft;
        set => SetField(ref installationRootWorldOfWarcraft, value);
    }

    public ObservableCollection<string> AccountnamesFromInstallationDirectory
    {
        get => accountnamesFromInstallationDirectory;
        set => SetField(ref accountnamesFromInstallationDirectory, value);
    }

    public string? SelectedAccount
    {
        get => selectedAccount;
        set => SetField(ref selectedAccount, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SpreadsheetServiceOnScanningProgressed(object sender, double progress)
    {
        ProgessbarValue = progress;
    }

    private async Task StartUpdatePrices()
    {
        if (StartUpdatePricesInProgress)
            return;

        StartUpdatePricesInProgress = true;

        await Task.Run(() =>
        {
            try
            {
                spreadsheetService.UpdateSpreadsheet(InstallationRootWorldOfWarcraft);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Spreadsheet schließen bitte.", "InFoRmAtIoN", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            finally
            {
                StartUpdatePricesInProgress = false;
            }
        });
    }

    private async Task OpenSpreadsheet()
    {
        if (OpenSpreadsheetInProgress)
            return;

        OpenSpreadsheetInProgress = true;

        await Task.Run(() => { spreadsheetService.OpenSpreadsheet(); });

        OpenSpreadsheetInProgress = false;
    }

    private async Task ExecuteFilePicker()
    {
        if (OpenFilePickerInProgress)
            return;

        OpenSpreadsheetInProgress = true;

        try
        {
            if (fileDialogService is FileDialogService service)
                InstallationRootWorldOfWarcraft = await service.SelectWoWInstallationRoot();
            else
                InstallationRootWorldOfWarcraft = await fileDialogService.SelectDirectory();

            var accountnames = await fileDialogService.GetAccountNames();
            AccountnamesFromInstallationDirectory = new ObservableCollection<string>(accountnames);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Halt Stop", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            OpenSpreadsheetInProgress = false;
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(SelectedAccount) && !string.IsNullOrWhiteSpace(SelectedAccount))
            InstallationRootWorldOfWarcraft =
                InstallationRootWorldOfWarcraft?.Replace(FileDialogService.AccountNamePlaceholder, SelectedAccount);
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}