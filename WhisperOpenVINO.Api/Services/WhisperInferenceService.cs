using Whisper.net;
using System.Text;

namespace WhisperOpenVINO.Api.Services;

/// <summary>
/// 封裝標準 Whisper 推論邏輯 (相容性最佳化)。
/// </summary>
public class WhisperInferenceService(ModelManagerService modelManager, ILogger<WhisperInferenceService> logger) : IDisposable
{
    private readonly WhisperFactory _factory = WhisperFactory.FromPath(modelManager.GetModelPath());
    private string? _detectedDevice;

    public async Task<string> TranscribeAsync(string wavPath, CancellationToken ct)
    {
        var device = GetBestDevice();
        try
        {
            using var processor = _factory.CreateBuilder()
                .WithLanguage("auto")
                .WithOpenVinoEncoder(modelManager.GetOpenVinoXmlPath(), device, null)
                .Build();

            using var fileStream = File.OpenRead(wavPath);
            var resultText = new StringBuilder();

            logger.LogInformation("開始執行語音辨識 (OpenVINO 加速模式，使用裝置: {Device})...", device);
            await foreach (var segment in processor.ProcessAsync(fileStream, ct))
            {
                resultText.Append(segment.Text);
            }

            return resultText.ToString().Trim();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "使用裝置 {Device} 進行轉錄時發生致命錯誤，清除偵測快取。", device);
            _detectedDevice = null; // 下次重新偵測
            throw;
        }
    }

    private string GetBestDevice()
    {
        if (_detectedDevice != null) return _detectedDevice;

        // 優先順序: NPU -> GPU -> CPU
        var devicesToTry = new[] { "NPU", "GPU" };
        
        foreach (var device in devicesToTry)
        {
            try
            {
                logger.LogInformation("正在嘗試探測 OpenVINO 裝置: {Device}...", device);
                
                // 嘗試建立處理器
                var builder = _factory.CreateBuilder()
                    .WithOpenVinoEncoder(modelManager.GetOpenVinoXmlPath(), device, null);
                
                using var testProcessor = builder.Build();
                
                // 如果到這裡沒崩潰，且沒拋出異常，暫且認為成功
                logger.LogInformation("成功初始化 OpenVINO 裝置: {Device}", device);
                _detectedDevice = device;
                return device;
            }
            catch (Exception ex)
            {
                logger.LogWarning("裝置 {Device} 初始化失敗: {Message}", device, ex.Message);
            }
        }

        logger.LogWarning("所有高效能裝置均不可用，使用 CPU 模式。");
        _detectedDevice = "CPU";
        return _detectedDevice;
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
