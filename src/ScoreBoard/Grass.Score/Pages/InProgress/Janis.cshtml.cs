using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Grass.Score.Pages;
public class JanisInProgress : PageModel
{
	private readonly ILogger<JanisInProgress> _logger;

	public JanisInProgress( ILogger<JanisInProgress> logger )
	{
		_logger = logger;
	}

	public void OnGet()
	{
	}
}
