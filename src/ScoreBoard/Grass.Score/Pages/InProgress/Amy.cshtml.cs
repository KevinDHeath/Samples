using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class AmyInProgress : PageModel
{
	private readonly ILogger<AmyInProgress> _logger;

	public AmyInProgress( ILogger<AmyInProgress> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
