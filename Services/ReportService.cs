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
        string path = Path.Combine(directory, "ai-building-report.pdf");
        WriteSimplePdf(path, BuildLines(model, errors, BuildAISection(model, errors)));
        return path;
    }

    private static IReadOnlyList<string> BuildLines(BuildingModel model, IReadOnlyList<ModelError> errors, AIReportSection aiSection)
    {
        var lines = new List<string>
        {
            "AI Report",
            string.Empty,
            $"Organization: {model.Organization}",
            $"Project: {model.Project}",
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}",
            string.Empty,
            "Statistics:",
            $"Floors: {model.LevelCount}",
            $"Walls: {model.Statistics.Walls}",
            $"Doors: {model.Statistics.Doors}",
            $"Windows: {model.Statistics.Windows}",
            $"Columns: {model.Columns.Count}",
            $"Rooms: {model.Statistics.Rooms}",
            $"Families: {model.Families.Count}",
            $"Views: {model.Views.Count}",
            $"Sheets: {model.Sheets.Count}",
            $"Dimensions: {model.Dimensions.Count}",
            $"Building area: {model.Statistics.BuildingArea.ToString("0.##", CultureInfo.InvariantCulture)} m2",
            string.Empty,
            "Element Counts:",
            $"Total tracked elements: {GetTrackedElementCount(model)}",
            $"Validation issues: {errors.Count}",
            $"Errors: {errors.Count(error => error.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase))}",
            $"Warnings: {errors.Count(error => error.Severity.Equals("Warning", StringComparison.OrdinalIgnoreCase))}",
            $"Info: {errors.Count(error => error.Severity.Equals("Info", StringComparison.OrdinalIgnoreCase))}",
            string.Empty,
            "Errors:"
        };

        if (errors.Count == 0)
        {
            lines.Add("No model errors found.");
        }
        else
        {
            lines.AddRange(errors.Take(14).Select(error => $"{error.Severity} {error.RuleId}: {error.ElementType} #{error.ElementId}: {error.Message}"));
        }

        lines.Add(string.Empty);
        lines.Add("AI Recommendations:");
        lines.AddRange(aiSection.Recommendations);
        lines.Add(string.Empty);
        lines.Add("Problem Zones:");
        lines.AddRange(aiSection.ProblemZones);

        return lines;
    }

    private static AIReportSection BuildAISection(BuildingModel model, IReadOnlyList<ModelError> errors)
    {
        var recommendations = new List<string>();
        var problemZones = new List<string>();

        if (errors.Any(error => error.RuleId.Contains("MISSING-WALL-MATERIAL", StringComparison.OrdinalIgnoreCase)))
        {
            recommendations.Add("Assign approved Maybeworks materials to walls before documentation release.");
        }

        if (errors.Any(error => error.RuleId.Contains("EMPTY-DOOR-MARK", StringComparison.OrdinalIgnoreCase) || error.RuleId.Contains("DUPLICATE-DOOR-MARK", StringComparison.OrdinalIgnoreCase)))
        {
            recommendations.Add("Run the AI action RenameDoorsByCompanyStandard and review duplicate door marks.");
        }

        if (errors.Any(error => error.RuleId.Contains("INVALID-LEVEL", StringComparison.OrdinalIgnoreCase)))
        {
            recommendations.Add("Review element level constraints before coordination export.");
        }

        if (errors.Any(error => error.RuleId.Contains("UNUSED-TYPE", StringComparison.OrdinalIgnoreCase)))
        {
            recommendations.Add("Purge or archive unused types to reduce model noise.");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("No critical recommendations. Continue with scheduled BIM QA review.");
        }

        problemZones.AddRange(errors
            .GroupBy(error => error.ElementType)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .Select(group => $"{group.Key}: {group.Count()} issue(s)."));

        problemZones.AddRange(model.Windows
            .Where(window => string.IsNullOrWhiteSpace(window.Level))
            .Take(3)
            .Select(window => $"Window #{window.Id}: missing level."));

        problemZones.AddRange(model.Walls
            .Where(wall => string.IsNullOrWhiteSpace(wall.Material))
            .Take(3)
            .Select(wall => $"Wall #{wall.Id}: missing material."));

        if (problemZones.Count == 0)
        {
            problemZones.Add("No concentrated problem zones detected from current validation data.");
        }

        return new AIReportSection
        {
            Recommendations = recommendations,
            ProblemZones = problemZones
        };
    }

    private static int GetTrackedElementCount(BuildingModel model) =>
        model.Walls.Count
        + model.Doors.Count
        + model.Windows.Count
        + model.Columns.Count
        + model.Rooms.Count
        + model.Levels.Count
        + model.Families.Count
        + model.Views.Count
        + model.Sheets.Count
        + model.Dimensions.Count;

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

        foreach (string line in lines.Take(42))
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
