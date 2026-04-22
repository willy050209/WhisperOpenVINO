using WhisperOpenVINO.Api.Models;
using WhisperOpenVINO.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WhisperOpenVINO.Api.Endpoints;

public static class TranscriptionEndpoints
{
    public static void MapTranscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transcribe", async (
            IFormFile file,
            AudioConversionService conversionService,
            WhisperInferenceService whisperService,
            CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("未提供音訊檔案。");
            }

            string? tempWavPath = null;
            try
            {
                var extension = Path.GetExtension(file.FileName);
                using var stream = file.OpenReadStream();
                
                // 1. 轉檔為 16kHz WAV
                tempWavPath = await conversionService.ConvertToWavAsync(stream, extension, ct);

                // 2. 進行推論
                var text = await whisperService.TranscribeAsync(tempWavPath, ct);

                return Results.Ok(new TranscriptionResponse(
                    Text: text,
                    DurationSeconds: 0, // 可進一步從 FFmpeg 取得時長
                    Language: "auto"
                ));
            }
            catch (Exception ex)
            {
                return Results.Problem($"轉錄過程中發生錯誤: {ex.Message}");
            }
            finally
            {
                if (tempWavPath != null && File.Exists(tempWavPath))
                {
                    File.Delete(tempWavPath);
                }
            }
        })
        .DisableAntiforgery()
        .WithSummary("將音訊轉換為文字")
        .WithDescription("上傳任意音訊格式 (MP3, WAV, M4A等)，服務將自動轉碼並利用 OpenVINO 加速進行轉錄。");
    }
}
