//using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alvianda.Software.Service.iCOMD.Data
{
    public class PaginatedList<T>
    {
        public PaginatedList()
        {

        }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int MaxRecords { get; set; }
        public List<T> Items { get; set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.Items = new List<T>();
            this.Items.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
            set { }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
            set { }
        }

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
