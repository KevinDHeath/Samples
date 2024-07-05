using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class BobGameOver : PageModel
{
	private readonly ILogger<BobGameOver> _logger;

	public BobGameOver( ILogger<BobGameOver> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
