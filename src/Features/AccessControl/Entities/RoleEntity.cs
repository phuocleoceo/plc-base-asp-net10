using System.ComponentModel.DataAnnotations.Schema;
using PlcBase.Base.Entity;
using PlcBase.Features.User.Entities;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.AccessControl.Entities;

[Table(TableName.ROLE)]
public class RoleEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    public ICollection<UserAccountEntity> UserAccounts { get; set; }

    public ICollection<PermissionEntity> Permissions { get; set; }
}
