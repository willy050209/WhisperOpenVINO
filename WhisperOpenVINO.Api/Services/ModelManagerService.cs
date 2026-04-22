using System.IO.Compression;
using Whisper.net.Ggml;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace WhisperOpenVINO.Api.Services;

/// <summary>
/// 啟動時負責確保 Whisper 模型與 FFmpeg 執行檔已準備就緒。
/// </summary>
public class ModelManagerService(ILogger<ModelManagerService> logger) : IHostedService
{
    private const string ModelZipName = "ggml-base-models.zip";
    private const string ModelFileName = "ggml-base.bin";
    private const string OpenVinoXmlName = "ggml-base-encoder-openvino.xml";

    private readonly string _modelFolder = Path.Combine(AppContext.BaseDirectory, "Models");
    private readonly string _modelPath = Path.Combine(AppContext.BaseDirectory, "Models", ModelFileName);
    private readonly string _openVinoXmlPath = Path.Combine(AppContext.BaseDirectory, "Models", OpenVinoXmlName);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("正在檢查系統環境...");

        // 1. 確保 FFmpeg 已安裝
        var ffmpegPath = AppContext.BaseDirectory;
        if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) && !File.Exists(Path.Combine(ffmpegPath, "ffmpeg")))
        {
            logger.LogInformation("未偵測到 FFmpeg，正在下載至 {Path}...", ffmpegPath);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
            logger.LogInformation("FFmpeg 下載完成。");
        }
        
        // 明確設定 FFmpeg 執行檔路徑
        FFmpeg.SetExecutablesPath(ffmpegPath);

        // 2. 確保目錄存在
        if (!Directory.Exists(_modelFolder)) Directory.CreateDirectory(_modelFolder);

        // 3. 確保 Whisper 模型已存在
        await EnsureModelFilesExistAsync(cancellationToken);
    }

    private async Task EnsureModelFilesExistAsync(CancellationToken ct)
    {
        // 改用官方穩定版下載器，確保模型檔案沒問題
        if (!File.Exists(_modelPath))
        {
            logger.LogInformation("正在從官方下載標準 Whisper base 模型...");
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base, cancellationToken: ct);
            using var fileStream = File.Create(_modelPath);
            await modelStream.CopyToAsync(fileStream, ct);
            logger.LogInformation("標準模型下載完成。");
        }

        // OpenVINO 檔案若不存在則下載 (僅作為加速備案，若報錯我們先用標準版跑通)
        if (!File.Exists(_openVinoXmlPath))
        {
            logger.LogInformation("正在嘗試下載 OpenVINO Encoder 備案檔案...");
            // 此處維持原 logic 或是先跳過以利排錯
            logger.LogInformation("目前優先測試標準模式，暫緩下載 OpenVINO 專用組件。");
        }
    }

    private async Task DownloadFileAsync(string url, string path, CancellationToken ct)
    {
        using var client = new HttpClient();
        // 設定較長的逾時時間，因為模型檔案較大
        client.Timeout = TimeSpan.FromMinutes(10);
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        
        using var fs = File.Create(path);
        await response.Content.CopyToAsync(fs, ct);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public string GetModelPath() => _modelPath;
    public string GetOpenVinoXmlPath() => _openVinoXmlPath;
}
