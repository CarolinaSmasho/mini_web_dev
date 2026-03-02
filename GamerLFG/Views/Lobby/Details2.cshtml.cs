using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GamerLFG.Views.Lobby
{
    public class Details2 : PageModel
    {
        private readonly ILogger<Details2> _logger;

        public Details2(ILogger<Details2> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}