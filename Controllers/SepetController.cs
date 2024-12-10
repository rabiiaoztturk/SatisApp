using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SatisApp.Models;

namespace SatisApp.Controllers
{
    public class SepetController : Controller
    {
        string connectionString = "";

        public IActionResult Index(string? MessageCssClass, string? Message)
        {
            ViewData["Nickname"] = HttpContext.Session.GetString("Nickname"); // // Oturumdan nickname alınır
            using var connection = new SqlConnection(connectionString);
            var sepet = connection.Query<Sepet>("SELECT * FROM Basket").ToList();

            ViewBag.Message = Message;
            ViewBag.MessageCssClass = MessageCssClass;

            return View(sepet);

        }
        public IActionResult SepetSil(int Id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Basket WHERE Id = @Id";
            var rowEffected = connection.Execute(sql, new { Id = Id });

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Ürün sepetten başarılı bir şekilde silindi.";

            return View("Message");
        }
        public IActionResult KrediOde()
        {
            using var connection = new SqlConnection(connectionString);

            var sepet = connection.Query<Sepet>("Select * From Basket").ToList();
            if (sepet.Count > 0)
            {
                return View();
            }

            ViewBag.MessageCssClass = "alert-danger";
            ViewBag.Message = "Sepetinizde Hiç Ürün Yok.";

            return View("Message");
        }
    }
}
