using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SatisApp.Models;

namespace SatisApp.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = "";

        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var sql = @"SELECT p.*, c.Name AS CategoryName FROM Products p INNER JOIN Categories c ON p.CategoryId = c.Id";

            var products = connection.Query<Product>(sql).ToList();

            return View(products);
        }

        public IActionResult AddProduct()
        {
            using var connection = new SqlConnection(connectionString);

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Ürün eklenirken bir hata oluştu";
                ViewBag.MessageCssClass = "alert-danger";
                return View("Message");
            }

            var ImageName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImgFile.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", ImageName);

            using var stream = new FileStream(path, FileMode.Create);
            model.ImgFile.CopyTo(stream);

            model.Image = ImageName;

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO Products (Name, Image, Price, Stock,Created,CategoryId) VALUES (@Name, @Image, @Price,@Stock, @Created,@CategoryId)";


            var data = new
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                Image = model.Image,
                Price = model.Price,
                Stock = model.Stock,
                Created = DateTime.Now
            };

            var rowsAffected = connection.Execute(sql, data);
            ViewBag.Message = "Ürün eklendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        public IActionResult Sil(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Products WHERE Id = @Id";
            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var product = connection.QuerySingleOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });

            return View(product);
        }

        [HttpPost]
        public IActionResult Duzenle(Product model)
        {
            using var connection = new SqlConnection(connectionString);

            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImgFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);

                using var stream = new FileStream(path, FileMode.Create);
                model.ImgFile.CopyTo(stream);

                model.Image = imageName;
            }

            var sql = "UPDATE Products SET Name = @Name, CategoryId = @CategoryId, Stock = @Stock, Price = @Price";

            if (model.ImgFile != null && model.ImgFile.Length > 0)
            {
                sql += ", Image = @Image";
            }

            sql += " WHERE Id = @Id";

            var param = new
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                Stock = model.Stock,
                Price = model.Price,
                Image = model.Image,
                Id = model.Id
            };

            var affectedRows = connection.Execute(sql, param);

            ViewBag.Message = "Ürün güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        [Route("/YorumSil/{Id}")]
        public IActionResult DeleteYorum(int Id, int tweetId)
        {
            using var connection = new SqlConnection(connectionString);


            var sql = "DELETE FROM comments WHERE Id = @Id";
            var rowsAffected = connection.Execute(sql, new { Id = Id });

            return RedirectToAction("Tweet", new { Id = tweetId });
        }

        [Route("/tweetsil/{Id}")]
        public IActionResult TweetSil(int Id, string nickname)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "DELETE FROM tweets WHERE Id = @Id";
            var rowsAffected = connection.Execute(sql, new { Id = Id });

            return RedirectToAction("Profile", new { nickname });
        }
        public IActionResult Comment()
        {
            return View();
        }
    }
}
