using PlcBase.Base.DomainModel;
using PlcBase.Common.Repositories;
using PlcBase.Features.Address.DTOs;
using PlcBase.Features.Address.Entities;

namespace PlcBase.Features.Address.Services;

public class AddressService(IUnitOfWork uow) : IAddressService
{
    public async Task<List<ProvinceDTO>> GetProvinces()
    {
        return await uow.AddressProvince.GetManyAsync<ProvinceDTO>();
    }

    public async Task<List<DistrictDTO>> GetDistricsOfProvince(int provinceId)
    {
        return await uow.AddressDistrict.GetManyAsync<DistrictDTO>(
            new QueryModel<AddressDistrictEntity>
            {
                Filters = { d => d.AddressProvinceId == provinceId },
            }
        );
    }

    public async Task<List<WardDTO>> GetWardsOfDistrict(int districtId)
    {
        return await uow.AddressWard.GetManyAsync<WardDTO>(
            new QueryModel<AddressWardEntity>
            {
                Filters = { w => w.AddressDistrictId == districtId },
            }
        );
    }

    public async Task<FullAddressDTO> GetFullAddressByWardId(int wardId)
    {
        return await uow.AddressWard.GetOneAsync<FullAddressDTO>(
            new QueryModel<AddressWardEntity>
            {
                Filters = { w => w.Id == wardId },
                Includes = { w => w.AddressDistrict.AddressProvince },
            }
        );
    }
}
