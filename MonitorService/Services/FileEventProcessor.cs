using MonitorService.Interfaces;
using MonitorService.Models;

namespace MonitorService.Services;

public class FileEventProcessor(
    IFileEventLogger logger,
    ILogger<FileEventProcessor> log,
    IConfigurationService configurationService) : IFileEventProcessor
{
    private readonly IFileEventLogger _logger = logger;
    private readonly ILogger<FileEventProcessor> _log = log;
    private readonly IConfigurationService _configurationService = configurationService;

    public async Task ProcessEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _log.LogDebug("Processing file event: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);

            // Aplicar filtros se necessário
            if (!ShouldProcessFile(fileEvent))
            {
                _log.LogDebug("File event filtered out: {EventType} - {FileName}", fileEvent.EventType, fileEvent.FileName);
                return;
            }

            // Log do evento
            await _logger.LogEventAsync(fileEvent, cancellationToken);

            // Processar evento específico
            switch (fileEvent.EventType)
            {
                case FileEventType.Created:
                    await ProcessCreatedEventAsync(fileEvent, cancellationToken);
                    break;
                case FileEventType.Deleted:
                    await ProcessDeletedEventAsync(fileEvent, cancellationToken);
                    break;
                case FileEventType.Changed:
                    await ProcessChangedEventAsync(fileEvent, cancellationToken);
                    break;
                case FileEventType.Renamed:
                    await ProcessRenamedEventAsync(fileEvent, cancellationToken);
                    break;
                case FileEventType.Error:
                    await ProcessErrorEventAsync(fileEvent, cancellationToken);
                    break;
            }

            _log.LogInformation("File event processed successfully: {EventType} - {FileName}",
                fileEvent.EventType, fileEvent.FileName);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error processing file event: {EventType} - {FileName}",
                fileEvent.EventType, fileEvent.FileName);

            await _logger.LogErrorAsync($"Error processing file event: {ex.Message}", ex, cancellationToken);
            throw;
        }
    }

    public Task<bool> ShouldProcessEventAsync(FileEvent fileEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ShouldProcessFile(fileEvent))
            {
                return Task.FromResult(false);
            }

            if (IsTemporaryFile(fileEvent.FileName))
            {
                _log.LogDebug("Skipping temporary file: {FileName}", fileEvent.FileName);
                return Task.FromResult(false);
            }

            if (IsSystemFile(fileEvent.FileName))
            {
                _log.LogDebug("Skipping system file: {FileName}", fileEvent.FileName);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error checking if event should be processed: {EventType} - {FileName}",
                fileEvent.EventType, fileEvent.FileName);
            return Task.FromResult(false);
        }
    }

    private async Task ProcessCreatedEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        _log.LogInformation("File created: {FileName} (Size: {FileSize} bytes)",
            fileEvent.FileName, fileEvent.FileSize);

        // Aqui você pode adicionar lógica específica para arquivos criados
        // Por exemplo: validação de conteúdo, processamento, etc.
    }

    private async Task ProcessDeletedEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        _log.LogInformation("File deleted: {FileName}", fileEvent.FileName);

        // Aqui você pode adicionar lógica específica para arquivos deletados
        // Por exemplo: limpeza de recursos, notificações, etc.
    }

    private async Task ProcessChangedEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        _log.LogInformation("File changed: {FileName} (Size: {FileSize} bytes)",
            fileEvent.FileName, fileEvent.FileSize);

        // Aqui você pode adicionar lógica específica para arquivos alterados
        // Por exemplo: verificação de integridade, reprocessamento, etc.
    }

    private async Task ProcessRenamedEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        _log.LogInformation("File renamed: {OldName} → {NewName}",
            fileEvent.OldName, fileEvent.NewName);

        // Aqui você pode adicionar lógica específica para arquivos renomeados
        // Por exemplo: atualização de índices, notificações, etc.
    }

    private async Task ProcessErrorEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        _log.LogError("File system error: {ErrorMessage}", fileEvent.ErrorMessage);

        // Aqui você pode adicionar lógica específica para erros
        // Por exemplo: alertas, recuperação, etc.
    }

    private bool ShouldProcessFile(FileEvent fileEvent)
    {
        var config = _configurationService.GetFileWatcherConfiguration();

        // Verificar filtros de arquivo
        if (!string.IsNullOrEmpty(config.FileFilters) && config.FileFilters != "*.*")
        {
            var filters = config.FileFilters.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var shouldProcess = filters.Any(filter =>
                fileEvent.FileName.EndsWith(filter.Trim(), StringComparison.OrdinalIgnoreCase));

            if (!shouldProcess)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsTemporaryFile(string fileName)
    {
        var tempExtensions = new[] { ".tmp", ".temp", "~" };
        var tempPrefixes = new[] { "~$", ".~" };

        return tempExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) ||
               tempPrefixes.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSystemFile(string fileName)
    {
        var systemFiles = new[] { "Thumbs.db", "desktop.ini", ".DS_Store" };
        return systemFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase);
    }
}
