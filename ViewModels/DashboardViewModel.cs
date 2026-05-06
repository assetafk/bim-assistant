using System.Collections.ObjectModel;
using BimAiAssistant.Models;
using BimAiAssistant.Utils;

namespace BimAiAssistant.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    public DashboardViewModel(BuildingModel model, IReadOnlyList<DashboardMetric> metrics)
    {
        Organization = model.Organization;
        Project = model.Project;
        Metrics = new ObservableCollection<DashboardMetric>(metrics);
    }

    public string Organization { get; }
    public string Project { get; }
    public ObservableCollection<DashboardMetric> Metrics { get; }
}
