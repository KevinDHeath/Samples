using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class BobInProgress : PageModel
{
	private readonly ILogger<BobInProgress> _logger;

	public BobInProgress( ILogger<BobInProgress> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
