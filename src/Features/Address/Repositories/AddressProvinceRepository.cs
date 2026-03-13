using AutoMapper;
using PlcBase.Base.Repository;
using PlcBase.Common.Data.Context;
using PlcBase.Features.Address.Entities;

namespace PlcBase.Features.Address.Repositories;

public class AddressProvinceRepository(DataContext db, IMapper mapper)
    : BaseRepository<AddressProvinceEntity>(db, mapper),
        IAddressProvinceRepository { }
