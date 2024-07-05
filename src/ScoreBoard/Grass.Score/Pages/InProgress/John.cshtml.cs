using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class JohnInProgress : PageModel
{
	private readonly ILogger<JohnInProgress> _logger;

	public JohnInProgress( ILogger<JohnInProgress> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
