using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources.NetStandard;
using System.Text;
using System.Xml.Linq;
using LocoMat.Translation;
using Microsoft.Extensions.Logging;

namespace LocoMat;

public class ResourceGenerator
{
    private readonly ConfigurationData _config;
    private readonly ILogger<ResourceGenerator> _logger;
    private readonly Translator _translator;
    private readonly BackupService _backupService;

    public ResourceGenerator(ConfigurationData config, ILogger<ResourceGenerator> logger, Translator translator, BackupService backupService)
    {
        _config = config;
        _logger = logger;
        _translator = translator;
        _backupService = backupService;
    }

    public void CreateResxFile(Dictionary<string, string> resourceKeys)
    {
        Utilities.EnsureFolderExists(_config.ResourcePath);

        var filePath = _config.ResourcePath;
        var existingResources = GetOrCreateResxFile(filePath);

        foreach (var resource in resourceKeys)
            existingResources.TryAdd(resource.Key, resource.Value);

        if (!_config.TestMode)
        {
            Utilities.WriteResourcesToFile(existingResources, filePath);
            _logger.LogInformation($"Created resource: {filePath}");
        }

        return;
    }

    public async Task TranslateResourceFile()
    {
        //Check if languages are not empty
        if (string.IsNullOrEmpty(_config.TargetLanguages)) return;

        var baseFileName = _config.ResourcePath;

        var existingResources = Utilities.GetExistingResources(baseFileName);

        foreach (var languageCode in _config.TargetLanguages.Split(','))
        {
            var translatedResources = GetOrCreateResxFile(baseFileName, languageCode);

            var errorCounter = 0;
            foreach (var resource in existingResources)
                if (!translatedResources.ContainsKey(resource.Key))
                {
                    var translate = resource.Value;
                    if (string.IsNullOrEmpty(translate))
                    {
                        _logger.LogDebug($"Skipping empty resource: {resource.Key}");
                        continue;
                    }

                    if (_config.TestMode)
                    {
                        translate = resource.Value;
                        _logger.LogDebug($"Translating resource:({languageCode}) {resource.Key} {translate}");
                    }
                    else
                    {
                        Result<string> result;
                        result = await _translator.Translate(resource.Value, languageCode);
                        if (result.IsSuccess)
                        {
                            translate = result.Value;
                            translatedResources.TryAdd(resource.Key, translate);
                            errorCounter = 0;
                        }
                        else
                        {
                            errorCounter++;
                            if (errorCounter > 5)
                            {
                                _logger.LogError("Stopping translation due to too many errors.");
                                return;
                            }
                        }
                    }
                }

            if (!_config.TestMode) Utilities.WriteResourcesToFile(translatedResources, baseFileName, languageCode);
        }
    }


    public Dictionary<string, string> GetOrCreateResxFile(string fileName, string language = "")
    {
        //Ensure than filename has correct extension
        fileName = Path.ChangeExtension(fileName, string.IsNullOrEmpty(language) ? ".resx" : $".{language}.resx");
        if (!File.Exists(fileName)) Utilities.CreateResxFileWithHeaders(fileName);
        return Utilities.GetExistingResources(fileName);
    }
}