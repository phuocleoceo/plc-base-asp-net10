using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Issue.Entities;

namespace PlcBase.Features.Issue.Repositories;

public class IssueCommentRepository(DataContext db, IMapper mapper)
    : BaseRepository<IssueCommentEntity>(db, mapper),
        IIssueCommentRepository
{
    public async Task<IssueCommentEntity> GetForUpdateAndDelete(
        int userId,
        int issueId,
        int commentId
    )
    {
        return await GetOneAsync<IssueCommentEntity>(
            new QueryModel<IssueCommentEntity>()
            {
                Filters = { i => i.Id == commentId && i.IssueId == issueId && i.UserId == userId },
            }
        );
    }
}
