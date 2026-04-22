# WhisperOpenVINO.Api

一個基於 ASP.NET Core 10 的高性能語音轉文字 (STT) 服務，整合了 OpenAI Whisper 與 Intel OpenVINO 加速技術。

## 🚀 特色

- **高性能辨識**：支援 Intel OpenVINO 硬體加速（CPU/iGPU/Arc GPU）。
- **自動環境整備**：啟動時自動檢查並下載必要的 Whisper 模型與 FFmpeg 執行檔。
- **格式自動相容**：內建音訊轉碼服務，支援上傳 MP3, WAV, M4A, FLAC 等多種格式。
- **現代化 API**：使用 .NET 10 Minimal API 架構，簡潔高效。
- **視覺化測試**：內建 Swagger UI，方便直接在瀏覽器進行檔案上傳與測試。

## 🛠 技術棧

- **框架**: .NET 10 (Minimal API)
- **語音辨識**: [Whisper.net](https://github.com/sandrohanea/whisper.net)
- **硬體加速**: OpenVINO Runtime (Intel)
- **音訊處理**: Xabe.FFmpeg
- **API 文件**: Swashbuckle (Swagger)

## 📦 安裝與執行

1. **複製專案**
   ```bash
   git clone <your-repo-url>
   cd WhisperOpenVINO.Api
   ```

2. **執行服務**
   ```bash
   dotnet run
   ```
   服務預設啟動於 `http://localhost:5251` (具體埠號請參考啟動日誌)。

3. **測試 API**
   - 瀏覽器打開 `http://localhost:5251/swagger` 進行視覺化測試。
   - 或使用 `curl`：
     ```bash
     curl -X POST -F "file=@audio.mp3" http://localhost:5251/api/transcribe
     ```

## ⚠️ 故障排除：GGML_ASSERT 異常

若在執行推論時遇到 `GGML_ASSERT(prev != ggml_ubcaught_exception) failed` 報錯：

1. **硬體相容性**：此錯誤通常源於 OpenVINO 與特定硬體或驅動程式的不相容。
2. **安全模式**：本專案目前的實作已切換至**標準相容模式**（純 CPU 推論），以確保在所有環境下皆能正常運作。
3. **開啟加速**：若需重新啟用 OpenVINO 加速，請修改 `WhisperInferenceService.cs` 並確保您的 Intel 驅動程式已更新至最新版本。

## 📝 授權

MIT License
