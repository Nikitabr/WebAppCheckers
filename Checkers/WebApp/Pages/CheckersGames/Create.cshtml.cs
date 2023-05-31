using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Pages.CheckersGames
{
    public class CreateModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;
        private readonly IGameRepository _repo;

        public CreateModel(DAL.Db.AppDbContext context, IGameRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public IActionResult OnGet()
        {
            OptionsSelectList = new SelectList(_context.CheckersOptions, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public CheckersGame CheckersGame { get; set; } = default!;

        public SelectList OptionsSelectList { get; set; } = default!;
        
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.CheckersGames == null || CheckersGame == null)
          {
              return Page();
          }
          
          _repo.AddGame(CheckersGame);
          
          return RedirectToPage("./Index");
        }
    }
}
