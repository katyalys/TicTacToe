using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TicTacToe.Models;

namespace TicTacToe.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}