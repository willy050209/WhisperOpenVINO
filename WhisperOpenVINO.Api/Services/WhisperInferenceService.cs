using Whisper.net;
using System.Text;

namespace WhisperOpenVINO.Api.Services;

/// <summary>
/// 封裝標準 Whisper 推論邏輯 (相容性最佳化)。
/// </summary>
public class WhisperInferenceService(ModelManagerService modelManager, ILogger<WhisperInferenceService> logger) : IDisposable
{
    private readonly WhisperFactory _factory = WhisperFactory.FromPath(modelManager.GetModelPath());

    public async Task<string> TranscribeAsync(string wavPath, CancellationToken ct)
    {
        using var processor = _factory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead(wavPath);
        var resultText = new StringBuilder();

        logger.LogInformation("開始執行語音辨識 (標準模式)...");
        await foreach (var segment in processor.ProcessAsync(fileStream, ct))
        {
            resultText.Append(segment.Text);
        }

        return resultText.ToString().Trim();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
