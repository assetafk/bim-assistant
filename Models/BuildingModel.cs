namespace BimAiAssistant.Models;

public sealed class BuildingModel
{
    public string Organization { get; set; } = "Maybeworks";
    public string Project { get; set; } = "Untitled";
    public int LevelCount { get; set; }
    public ModelStatistics Statistics { get; set; } = new(0, 0, 0, 0, 0, 0);
    public List<WallModel> Walls { get; set; } = [];
    public List<DoorModel> Doors { get; set; } = [];
    public List<WindowModel> Windows { get; set; } = [];
    public List<RoomModel> Rooms { get; set; } = [];
}
