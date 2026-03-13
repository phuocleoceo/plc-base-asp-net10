using PlcBase.Base.Repository;
using PlcBase.Features.Issue.Entities;

namespace PlcBase.Features.Issue.Repositories;

public interface IIssueCommentRepository : IBaseRepository<IssueCommentEntity>
{
    Task<IssueCommentEntity> GetForUpdateAndDelete(int userId, int issueId, int commentId);
}
