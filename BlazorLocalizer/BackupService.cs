using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorLocalizer;

public class BackupService : IDisposable
{
    private string _basePath;
    private string _backupPath;
    private readonly ILogger<RazorProcessor> _logger;
    private readonly ConfigurationData _config;

    // Lazy initialization of ZipArchive
    private ZipArchive _zipArchive;
    private ZipArchive ZipArchive => _zipArchive ??= CreateBackupFile();

    public BackupService(ILogger<RazorProcessor> logger, ConfigurationData config)
    {
        _logger = logger;
        _config = config;
        _basePath = Path.GetDirectoryName(_config.Project);
        _backupPath = Path.Combine(_basePath, ".LocalizerBackup");
    }

    private ZipArchive CreateBackupFile()
    {
        Directory.CreateDirectory(_backupPath);

        // Filename should contain sortable date and time
        var fileName = $"backup{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.zip";
        var zipFilePath = Path.Combine(_backupPath, fileName);
        _logger.LogInformation("Created backup file " + zipFilePath);
        return ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
    }

    public async Task WriteAllTextWithBackup(string path, string newContent)
    {
        if (_config.TestMode) return;

        //Check if file exists and content is different
        if (File.Exists(path) && await CalculateHashFromFileAsync(path) == CalculateHash(newContent))
        {
            _logger.LogDebug("Skipping unchanged file " + path);
            return;
        }

        var relativePath = Path.GetRelativePath(_basePath, path);
        var entry = ZipArchive.CreateEntryFromFile(path, relativePath);
        var hash = CalculateHash(newContent);
        entry.Comment = hash;
        await File.WriteAllTextAsync(path, newContent);
    }

    public async Task BackupFileAsync(string path)
    {
        if (_config.TestMode) return;
        //Check if file exists and content is different
        if (File.Exists(path))
        {
            var relativePath = Path.GetRelativePath(_basePath, path);
            var entry = ZipArchive.CreateEntryFromFile(path, relativePath);
            var hash = await CalculateHashFromFileAsync(path);
            entry.Comment = hash;
        }
    }

    public async Task UpdateFileHashAsync(string path)
    {
        if (_config.TestMode) return;
        //Check if file exists and content is different
        if (File.Exists(path))
        {
            var relativePath = Path.GetRelativePath(_basePath, path);
            var entry = ZipArchive.GetEntry(relativePath);
            if (entry != null)
            {
                var hash = await CalculateHashFromFileAsync(path);
                entry.Comment = hash;
            }
        }
    }

    private string CalculateHash(string content)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    //calculate hash from file
    private async Task<string> CalculateHashFromFileAsync(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }


    public async Task RestoreAsync()
    {
        var files = Directory.GetFiles(_backupPath, "*.zip");
        if (files.Length == 0)
        {
            _logger.LogError("No backup file found.");
            return;
        }


        var lastBackup = files.Select(f => new FileInfo(f)).OrderByDescending(f => f.CreationTime).First().FullName;
        using var zipArchive = ZipFile.Open(lastBackup, ZipArchiveMode.Read);
        _logger.LogInformation("Restoring backup file " + lastBackup);
        foreach (var entry in zipArchive.Entries)
        {
            var fullPath = Path.Combine(_basePath, entry.FullName);
            var directoryPath = Path.GetDirectoryName(fullPath);
            //check if file exists and if it has the same hash as the backup file restore it
            if (File.Exists(fullPath) && await CalculateHashFromFileAsync(fullPath) != entry.Comment)
            {
                _logger.LogInformation("Skipping modified file " + fullPath);
                continue;
            }

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            _logger.LogInformation("Restoring file " + fullPath);
            entry.ExtractToFile(fullPath, true);
        }
    }

    public void Close()
    {
        _logger.LogInformation("Closing backup file");
        _zipArchive?.Dispose();
        _zipArchive = null;
    }


    public void Dispose()
    {
        _zipArchive?.Dispose();
    }
}