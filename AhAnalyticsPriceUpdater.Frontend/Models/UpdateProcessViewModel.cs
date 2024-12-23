﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AhAnalyticsPriceUpdater.Services;

namespace AhAnalyticsPriceUpdater.Frontend.Models;

public class UpdateProcessViewModel : INotifyPropertyChanged
{
    private readonly SpreadsheetService spreadsheetService;

    public UpdateProcessViewModel(SpreadsheetService spreadsheetService)
    {
        this.spreadsheetService = spreadsheetService;

        StartUpdatePricesProcess = new AsyncRelayCommand(StartUpdatePrices);
        OpenSpreadsheetProcess   = new AsyncRelayCommand(OpenSpreadsheet);
    }

    public ICommand                           StartUpdatePricesProcess    { get; }
    public bool                               StartUpdatePricesInProgress { get; set; }
    public ICommand                           OpenSpreadsheetProcess      { get; }
    public bool                               OpenSpreadsheetInProgress   { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task StartUpdatePrices()
    {
        if (StartUpdatePricesInProgress)
            return;

        StartUpdatePricesInProgress = true;

        await Task.Run(() =>
        {
            spreadsheetService.UpdateSpreadsheet();
        });

        StartUpdatePricesInProgress = false;
    }

    private void OpenSpreadsheet() { }

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