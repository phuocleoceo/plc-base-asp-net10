using System.ComponentModel.DataAnnotations.Schema;
using PlcBase.Base.Entity;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.Address.Entities;

[Table(TableName.ADDRESS_PROVINCE)]
public class AddressProvinceEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    public ICollection<AddressDistrictEntity> AddressDistricts { get; set; }
}
