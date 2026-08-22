using BasiQBLL.DTOs.PaginationDTOs;

namespace BasiQBLL.Extensions
{
    public static class PaginationExtensions
    {
        public static PagedResultDTO<TDestination> ToPagedResult<TSource, TDestination>(
        this (IEnumerable<TSource> Data, int TotalCount) result,
        IEnumerable<TDestination> mappedItems,
        PaginationParametersDTO pagination)
        {
            return new PagedResultDTO<TDestination>
            {
                Items = mappedItems,
                TotalCount = result.TotalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }
    }
}
