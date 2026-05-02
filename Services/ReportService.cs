using System.Globalization;
using System.IO;
using System.Text;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class ReportService
{
    public string Generate(BuildingModel model, IReadOnlyList<ModelError> errors)
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BimAiAssistant");

        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "building-report.pdf");
        WriteSimplePdf(path, BuildLines(model, errors));
        return path;
    }

    private static IReadOnlyList<string> BuildLines(BuildingModel model, IReadOnlyList<ModelError> errors)
    {
        var lines = new List<string>
        {
            $"Organization: {model.Organization}",
            $"Project: {model.Project}",
            $"Floors: {model.LevelCount}",
            $"Walls: {model.Statistics.Walls}",
            $"Doors: {model.Statistics.Doors}",
            $"Windows: {model.Statistics.Windows}",
            $"Rooms: {model.Statistics.Rooms}",
            $"Building area: {model.Statistics.BuildingArea.ToString("0.##", CultureInfo.InvariantCulture)} m2",
            string.Empty,
            "Model errors:"
        };

        if (errors.Count == 0)
        {
            lines.Add("No model errors found.");
        }
        else
        {
            lines.AddRange(errors.Select(error => $"{error.ElementType} #{error.ElementId}: {error.Message}"));
        }

        return lines;
    }

    private static void WriteSimplePdf(string path, IReadOnlyList<string> lines)
    {
        var objects = new List<string>();
        string content = BuildPageContent(lines);
        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
        objects.Add("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\n" };

        writer.Write("%PDF-1.4\n");
        var offsets = new List<long> { 0 };

        for (int i = 0; i < objects.Count; i++)
        {
            writer.Flush();
            offsets.Add(stream.Position);
            writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }

        writer.Flush();
        long xref = stream.Position;
        writer.Write($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (long offset in offsets.Skip(1))
        {
            writer.Write($"{offset:0000000000} 00000 n \n");
        }

        writer.Write($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
    }

    private static string BuildPageContent(IReadOnlyList<string> lines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 20 Tf");
        builder.AppendLine("50 790 Td");
        builder.AppendLine($"({Escape("BIM AI Assistant Report")}) Tj");
        builder.AppendLine("/F1 12 Tf");
        builder.AppendLine("0 -30 Td");

        foreach (string line in lines.Take(36))
        {
            builder.AppendLine($"({Escape(line)}) Tj");
            builder.AppendLine("0 -18 Td");
        }

        builder.Append("ET");
        return builder.ToString();
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
