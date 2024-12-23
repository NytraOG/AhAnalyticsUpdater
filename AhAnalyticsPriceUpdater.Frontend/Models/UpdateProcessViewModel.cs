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
    private readonly IDialogService     fileDialogService;
    private readonly SpreadsheetService spreadsheetService;
    private          string?            installationRootWorldOfWarcraft;

    public UpdateProcessViewModel(SpreadsheetService spreadsheetService, IDialogService fileDialogService)
    {
        this.fileDialogService  = fileDialogService;
        this.spreadsheetService = spreadsheetService;

        StartUpdatePricesProcess = new AsyncRelayCommand(StartUpdatePrices);
        OpenSpreadsheetProcess   = new AsyncRelayCommand(OpenSpreadsheet);
        OpenFilePickerProcess    = new AsyncRelayCommand(OpenFilePicker);
    }

    public ICommand                           StartUpdatePricesProcess        { get; }
    public bool                               StartUpdatePricesInProgress     { get; set; }
    public ICommand                           OpenSpreadsheetProcess          { get; }
    public bool                               OpenSpreadsheetInProgress       { get; set; }
    public ICommand                           OpenFilePickerProcess           { get; }
    public bool                               OpenFilePickerInProgress        { get; set; }

    public string? InstallationRootWorldOfWarcraft
    {
        get => installationRootWorldOfWarcraft;
        set => SetField(ref installationRootWorldOfWarcraft, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task StartUpdatePrices()
    {
        if (StartUpdatePricesInProgress)
            return;

        StartUpdatePricesInProgress = true;

        await Task.Run(() =>
        {
            try
            {
                spreadsheetService.UpdateSpreadsheet();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Spreadsheet schließen bitte.", "InFoRmAtIoN", MessageBoxButton.OK, MessageBoxImage.Information);
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

        await Task.Run(() =>
        {
            spreadsheetService.OpenSpreadsheet();
        });

        OpenSpreadsheetInProgress = false;
    }

    private async Task OpenFilePicker()
    {
        if (OpenFilePickerInProgress)
            return;

        OpenSpreadsheetInProgress = true;

        if (fileDialogService is FileDialogService service)
            InstallationRootWorldOfWarcraft = await service.SelectWoWInstallationRoot();
        else
            InstallationRootWorldOfWarcraft = await fileDialogService.SelectDirectory();

        OpenSpreadsheetInProgress = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}