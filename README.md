# WhisperOpenVINO.Api

一個基於 ASP.NET Core 10 的高性能語音轉文字 (STT) 服務，整合了 OpenAI Whisper 與 Intel OpenVINO 加速技術。

## 🚀 特色

- **高性能辨識**：預設啟用 Intel OpenVINO 硬體加速（CPU/iGPU/Arc GPU）。
- **自動環境整備**：啟動時自動檢查並從 Intel Hugging Face 下載優化過的 Whisper 模型（包含 OpenVINO Encoder XML/BIN）與 FFmpeg 執行檔。
- **部署自動化**：內建 MSBuild 腳本，建置時自動部署 OpenVINO 原生執行庫，確保開箱即用。
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

## ✅ 已修復：GGML_ASSERT 異常

先前版本的 `GGML_ASSERT(prev != ggml_uncaught_exception) failed` 報錯已於本版本修復：
1. **補齊依賴**：加入了 `OpenVINO.runtime.win` 套件以提供完整的 OpenVINO 執行階段。
2. **自動部署**：透過 `.csproj` 的 `CopyOpenVinoDlls` 目標，自動將 `openvino.dll` 等原生庫複製到執行根目錄。
3. **穩定初始化**：優化了 `WhisperInferenceService` 的初始化流程，確保在推論前正確載入 OpenVINO Encoder。


## 📝 授權

MIT License
