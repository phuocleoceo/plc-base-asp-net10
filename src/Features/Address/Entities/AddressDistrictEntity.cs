using System.ComponentModel.DataAnnotations.Schema;
using PlcBase.Base.Entity;
using PlcBase.Shared.Enums;

namespace PlcBase.Features.Address.Entities;

[Table(TableName.ADDRESS_DISTRICT)]
public class AddressDistrictEntity : BaseEntity
{
    [Column("name")]
    public string Name { get; set; }

    [ForeignKey(nameof(AddressProvince))]
    [Column("province_id")]
    public int AddressProvinceId { get; set; }

    public AddressProvinceEntity AddressProvince { get; set; }

    public ICollection<AddressWardEntity> AddressWards { get; set; }
}
