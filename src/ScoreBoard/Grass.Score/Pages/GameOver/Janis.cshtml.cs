using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class JanisGameOver : PageModel
{
	private readonly ILogger<JanisGameOver> _logger;

	public JanisGameOver( ILogger<JanisGameOver> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
