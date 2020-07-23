using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace COMPortal.Data
{
    public class PaginationModel : PageModel
    {
        private IEventViewerService _eventViewerService;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int Count { get; set; }
        public int PageSize { get; set; } = 10;

        public string Category { get; set; }

        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Count, PageSize));

        public List<EventViewerEntry> Data { get; set; }

        public PaginationModel(IEventViewerService evservice)
        {
            _eventViewerService = evservice;
        }

        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;
        public bool ShowFirst => CurrentPage != 1;
        public bool ShowLast => CurrentPage != TotalPages;

        public async Task OnGetAsync()
        {
            Data = await _eventViewerService.GetPaginatedResult(Category,CurrentPage, PageSize);
            Count = await _eventViewerService.GetCount(Category);
        }

    }
}