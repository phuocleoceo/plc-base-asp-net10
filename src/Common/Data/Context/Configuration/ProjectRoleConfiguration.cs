using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlcBase.Features.ProjectAccess.Entities;

namespace PlcBase.Common.Data.Context.Configuration;

public class ProjectRoleConfiguration : IEntityTypeConfiguration<ProjectRoleEntity>
{
    public void Configure(EntityTypeBuilder<ProjectRoleEntity> builder) { }
}
