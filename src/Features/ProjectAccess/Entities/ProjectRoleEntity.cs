using System.ComponentModel.DataAnnotations.Schema;
using PlcBase.Base.Entity;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.ProjectAccess.Entities;

[Table(TableName.PROJECT_ROLE)]
public class ProjectRoleEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    public ICollection<ProjectPermissionEntity> ProjectPermissions { get; set; }

    public ProjectRoleEntity()
    {
        ProjectPermissions = new List<ProjectPermissionEntity>();
    }
}
