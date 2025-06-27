namespace LitraLand.Application.Services.Common.Governorates;
public interface IGovernorateService
{
    IEnumerable<Governorate> GetActiveGovernorates();
}