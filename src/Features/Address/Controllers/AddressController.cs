using Microsoft.AspNetCore.Mvc;
using PlcBase.Base.Controller;
using PlcBase.Base.DTO;
using PlcBase.Features.Address.DTOs;
using PlcBase.Features.Address.Services;

namespace PlcBase.Features.Address.Controllers;

public class AddressController(IAddressService addressService) : BaseController
{
    [HttpGet("Provinces")]
    public async Task<SuccessResponse<List<ProvinceDTO>>> GetProvinces()
    {
        return HttpContext.Success(await addressService.GetProvinces());
    }

    [HttpGet("Provinces/{provinceId}/Districts")]
    public async Task<SuccessResponse<List<DistrictDTO>>> GetDistrictsOfProvince(int provinceId)
    {
        return HttpContext.Success(await addressService.GetDistricsOfProvince(provinceId));
    }

    [HttpGet("Districts/{districtId}/Wards")]
    public async Task<SuccessResponse<List<WardDTO>>> GetWardsOfDistrict(int districtId)
    {
        return HttpContext.Success(await addressService.GetWardsOfDistrict(districtId));
    }

    [HttpGet("Full-Address/{wardId}")]
    public async Task<SuccessResponse<FullAddressDTO>> GetFullAddressByWardId(int wardId)
    {
        return HttpContext.Success(await addressService.GetFullAddressByWardId(wardId));
    }
}
