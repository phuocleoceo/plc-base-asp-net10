using System.ComponentModel.DataAnnotations.Schema;
using PlcBase.Base.Entity;
using PlcBase.Features.Project.Entities;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.ProjectStatus.Entities;

[Table(TableName.PROJECT_STATUS)]
public class ProjectStatusEntity : BaseSoftDeletableEntity
{
    [Column("name")]
    public string Name { get; set; }

    [Column("index")]
    public double Index { get; set; }

    [ForeignKey(nameof(Project))]
    [Column("project_id")]
    public int ProjectId { get; set; }
    public ProjectEntity Project { get; set; }
}
