namespace LitraLand.Application.Services.Common.Areas;
public interface IAreaService
{
    IEnumerable<Area> GetActiveAreasByGovernorateId(int id);
}