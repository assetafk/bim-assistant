using BimAiAssistant.Models;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace BimAiAssistant.Services;

public sealed class ExportService
{
    public IReadOnlyList<ExportResult> ExportBuilding(BuildingModel model, IReadOnlyList<ModelError> errors)
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "BimAiAssistant");

        Directory.CreateDirectory(directory);

        return
        [
            ExportJson(model, directory),
            ExportCsv(model, directory),
            ExportExcel(model, directory),
            ExportPdf(model, errors, directory)
        ];
    }

    public ExportResult ExportJson(BuildingModel model, string directory)
    {
        string path = Path.Combine(directory, "building.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(model, Formatting.Indented));
        return new ExportResult { Format = ExportFormat.Json, Path = path };
    }

    public ExportResult ExportCsv(BuildingModel model, string directory)
    {
        string path = Path.Combine(directory, "building.csv");
        var builder = new StringBuilder();
        builder.AppendLine("Category,Id,Name,Level,Material");

        foreach (WallModel wall in model.Walls)
        {
            builder.AppendLine(Csv("Wall", wall.Id, string.Empty, string.Empty, wall.Material));
        }

        foreach (DoorModel door in model.Doors)
        {
            builder.AppendLine(Csv("Door", door.Id, door.Mark, door.Level, string.Empty));
        }

        foreach (WindowModel window in model.Windows)
        {
            builder.AppendLine(Csv("Window", window.Id, string.Empty, window.Level, string.Empty));
        }

        foreach (ColumnModel column in model.Columns)
        {
            builder.AppendLine(Csv("Column", column.Id, column.TypeName, column.Level, column.Material));
        }

        foreach (RoomModel room in model.Rooms)
        {
            builder.AppendLine(Csv("Room", room.Id, room.Name, string.Empty, string.Empty));
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        return new ExportResult { Format = ExportFormat.Csv, Path = path };
    }

    public ExportResult ExportExcel(BuildingModel model, string directory)
    {
        string path = Path.Combine(directory, "building.xls");
        string xml = BuildSpreadsheetXml(model);
        File.WriteAllText(path, xml, Encoding.UTF8);
        return new ExportResult { Format = ExportFormat.Excel, Path = path };
    }

    public ExportResult ExportPdf(BuildingModel model, IReadOnlyList<ModelError> errors, string directory)
    {
        string path = new ReportService().Generate(model, errors);
        return new ExportResult { Format = ExportFormat.Pdf, Path = path };
    }

    private static string Csv(string category, long id, string name, string level, string material) =>
        $"{Escape(category)},{id},{Escape(name)},{Escape(level)},{Escape(material)}";

    private static string Escape(string value) =>
        $"\"{value.Replace("\"", "\"\"")}\"";

    private static string BuildSpreadsheetXml(BuildingModel model)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<?xml version=\"1.0\"?>");
        builder.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        builder.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        builder.AppendLine("<Worksheet ss:Name=\"Summary\"><Table>");
        AddRow(builder, "Metric", "Value");
        AddRow(builder, "Project", model.Project);
        AddRow(builder, "Walls", model.Walls.Count.ToString());
        AddRow(builder, "Doors", model.Doors.Count.ToString());
        AddRow(builder, "Windows", model.Windows.Count.ToString());
        AddRow(builder, "Columns", model.Columns.Count.ToString());
        AddRow(builder, "Rooms", model.Rooms.Count.ToString());
        AddRow(builder, "Levels", model.Levels.Count.ToString());
        builder.AppendLine("</Table></Worksheet>");

        builder.AppendLine("<Worksheet ss:Name=\"Elements\"><Table>");
        AddRow(builder, "Category", "Id", "Name", "Level", "Material");
        foreach (WallModel wall in model.Walls)
        {
            AddRow(builder, "Wall", wall.Id.ToString(), string.Empty, string.Empty, wall.Material);
        }

        foreach (DoorModel door in model.Doors)
        {
            AddRow(builder, "Door", door.Id.ToString(), door.Mark, door.Level, string.Empty);
        }

        foreach (WindowModel window in model.Windows)
        {
            AddRow(builder, "Window", window.Id.ToString(), string.Empty, window.Level, string.Empty);
        }

        builder.AppendLine("</Table></Worksheet></Workbook>");
        return builder.ToString();
    }

    private static void AddRow(StringBuilder builder, params string[] cells)
    {
        builder.Append("<Row>");
        foreach (string cell in cells)
        {
            builder.Append("<Cell><Data ss:Type=\"String\">");
            builder.Append(System.Security.SecurityElement.Escape(cell));
            builder.Append("</Data></Cell>");
        }

        builder.AppendLine("</Row>");
    }
}
