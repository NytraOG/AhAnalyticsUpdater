using AhAnalyticsPriceUpdater.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AhAnalyticsPriceUpdater.Tests;

public class Tests
{
    private Mock<ILogger<ScanDataDecrypter>>? loggerMock;
    private ScanDataDecrypter? scanDataDecrypter;

    [SetUp]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<ScanDataDecrypter>>();
        scanDataDecrypter = new ScanDataDecrypter(loggerMock.Object);
    }

    [Test]
    public void GetAllAuctions_ValidScanDataAvailable_ScanProgressionCulminatesTo1()
    {
        double totalProgress = 0;
        scanDataDecrypter.ScanningProgressed += (_, progress) => { totalProgress += progress; }; 
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "ScanDataSample\\Auc-ScanData.lua");
        scanDataDecrypter?.GetAllAuctions(directory);
        
        Assert.That(totalProgress, Is.EqualTo(1).Within(0.00001));
    }
}