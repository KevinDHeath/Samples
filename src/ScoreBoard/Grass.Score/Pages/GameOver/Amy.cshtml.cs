using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class AmyGameOver : PageModel
{
	private readonly ILogger<AmyGameOver> _logger;

	public AmyGameOver( ILogger<AmyGameOver> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}