using Microsoft.AspNetCore.Mvc;

namespace CallManagement.Controllers;

public class CallsPageController : Controller
{
    public IActionResult Index() => View();
}
