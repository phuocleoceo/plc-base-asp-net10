using AutoMapper;
using PlcBase.Base.DomainModel;
using PlcBase.Common.Repositories;
using PlcBase.Features.Media.DTOs;
using PlcBase.Features.Media.Entities;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.Media.Services;

public class MediaService(IUnitOfWork uow, IMapper mapper) : IMediaService
{
    public async Task<bool> CreateMedia(CreateMediaDTO createMediaDTO)
    {
        MediaEntity mediaEntity = mapper.Map<MediaEntity>(createMediaDTO);

        uow.Media.Add(mediaEntity);
        return await uow.Save();
    }

    public async Task<bool> CreateManyMedia(IEnumerable<CreateMediaDTO> createMediaDTOs)
    {
        IEnumerable<MediaEntity> mediaEntities = mapper.Map<IEnumerable<MediaEntity>>(
            createMediaDTOs
        );

        uow.Media.AddRange(mediaEntities);
        return await uow.Save();
    }

    public async Task<List<MediaDTO>> GetAllMediaOfIssue(int issueId)
    {
        return await uow.Media.GetManyAsync<MediaDTO>(
            new QueryModel<MediaEntity>()
            {
                Filters = { m => m.EntityId == issueId && m.EntityType == EntityType.ISSUE },
            }
        );
    }
}
