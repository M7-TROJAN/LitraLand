using LitraLand.Domain.Dtos;

namespace LitraLand.Web.Extensions
{
    public static class FormCollectionExtensions
    {
        public static GetFilteredDto GetFilters(this IFormCollection form)
        {
            var sortColumnIndex = form["order[0][column]"];

            GetFilteredDto filteredDto = new GetFilteredDto(
                    Skip: int.TryParse(form["start"], out int s) ? s : 0,
                    PageSize: int.TryParse(form["length"], out int p) ? p : 10,
                    SearchValue: form["search[value]"].ToString(),
                    SortColumnIndex: sortColumnIndex!,
                    SortColumn: form[$"columns[{sortColumnIndex}][name]"]!,
                    SortColumnDirection: form["order[0][dir]"]!
            );

            return filteredDto;
        }
    }
}
