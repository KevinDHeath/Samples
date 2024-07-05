using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class JohnGameOver : PageModel
{
	private readonly ILogger<JohnGameOver> _logger;

	public JohnGameOver( ILogger<JohnGameOver> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
