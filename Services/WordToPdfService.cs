using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using WordToPdfApp.Models;

namespace WordToPdfApp.Services;

/// <summary>
/// Word 转 PDF 核心服务
/// 基于 Syncfusion DocIO + DocIORenderer 实现纯本地离线转换
/// 无需安装 Office/WPS，支持 .doc / .docx
/// </summary>
public class WordToPdfService
{
    private readonly LogService _logger;

    public WordToPdfService(LogService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 转换单个 Word 文件为 PDF
    /// </summary>
    /// <param name="item">文件项</param>
    /// <param name="settings">转换设置</param>
    /// <param name="progress">进度回调（0-100）</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task ConvertAsync(FileItem item, ConvertSettings settings, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            item.Status = ConvertStatus.Converting;
            item.Progress = 0;
            item.Message = string.Empty;

            var sourcePath = item.FilePath;
            if (!File.Exists(sourcePath))
            {
                item.Status = ConvertStatus.Failed;
                item.Message = "源文件不存在";
                _logger.Error($"转换失败：{item.FileName} - 源文件不存在");
                return;
            }

            // 计算输出路径
            var outputDir = GetOutputDirectory(sourcePath, settings);
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            var outputFileName = Path.GetFileNameWithoutExtension(sourcePath) + ".pdf";
            var outputPath = Path.Combine(outputDir, outputFileName);

            // 重复文件策略
            if (File.Exists(outputPath) && !settings.OverwriteExisting)
            {
                item.Status = ConvertStatus.Skipped;
                item.Message = "文件已存在，已跳过";
                item.Progress = 100;
                _logger.Info($"已跳过：{item.FileName}（PDF 已存在）");
                return;
            }

            progress?.Report(10);

            // 在后台线程执行，避免阻塞 UI
            await Task.Run(() =>
            {
                try
                {
                    // 打开 Word 文档
                    using var stream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var document = new WordDocument(stream, GetFormat(item.Format));

                    progress?.Report(40);
                    cancellationToken.ThrowIfCancellationRequested();

                    // 转换为 PDF
                    using var renderer = new DocIORenderer();
                    using var pdfDocument = renderer.ConvertToPDF(document);

                    progress?.Report(80);
                    cancellationToken.ThrowIfCancellationRequested();

                    // 保存 PDF
                    using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                    pdfDocument.Save(outputStream);

                    progress?.Report(100);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"转换异常：{ex.Message}", ex);
                }
            }, cancellationToken);

            item.Status = ConvertStatus.Success;
            item.Progress = 100;
            item.Message = $"已保存到 {outputPath}";
            _logger.Success($"转换成功：{item.FileName}");
        }
        catch (OperationCanceledException)
        {
            item.Status = ConvertStatus.Failed;
            item.Message = "已取消";
            _logger.Warning($"已取消：{item.FileName}");
        }
        catch (Exception ex)
        {
            item.Status = ConvertStatus.Failed;
            item.Message = ex.Message;
            _logger.Error($"转换失败：{item.FileName} - {ex.Message}");
        }
    }

    /// <summary>
    /// 获取输出目录
    /// </summary>
    private string GetOutputDirectory(string sourcePath, ConvertSettings settings)
    {
        if (!settings.OutputToSource && !string.IsNullOrEmpty(settings.OutputDirectory))
        {
            return settings.OutputDirectory;
        }
        // 原目录下创建 PDF 文件夹
        var sourceDir = Path.GetDirectoryName(sourcePath) ?? string.Empty;
        return Path.Combine(sourceDir, "PDF输出");
    }

    /// <summary>
    /// 根据文件扩展名获取 Syncfusion 文档格式
    /// </summary>
    private static FormatType GetFormat(string extension)
    {
        var ext = extension.TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "doc" => FormatType.Doc,
            "docx" => FormatType.Docx,
            "rtf" => FormatType.Rtf,
            "txt" => FormatType.Txt,
            "html" => FormatType.Html,
            "md" => FormatType.Markdown,
            _ => FormatType.Automatic
        };
    }
}
