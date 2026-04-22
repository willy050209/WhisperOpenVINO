using WhisperOpenVINO.Api.Endpoints;
using WhisperOpenVINO.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. 註冊服務
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Whisper OpenVINO API", Version = "v1" });
});

builder.Services.AddSingleton<ModelManagerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ModelManagerService>());

builder.Services.AddTransient<AudioConversionService>();
// WhisperInferenceService 由於持有 WhisperFactory (Unmanaged 資源且載入重)，建議為 Singleton
builder.Services.AddSingleton<WhisperInferenceService>();

// 增加上傳限制 (例如支援到 50MB 的音檔)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024;
});

var app = builder.Build();

// 2. 啟用 Swagger 中間件 (開發環境與生產環境皆可，方便測試)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Whisper OpenVINO API v1");
    c.RoutePrefix = "swagger"; // 訪問網址為 http://localhost:port/swagger
});

// 3. 設定路由
app.MapTranscriptionEndpoints();

app.MapGet("/", () => "Whisper OpenVINO API is running.");

app.Run();
