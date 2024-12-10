using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SatisApp.Models;

namespace SatisApp.Controllers
{
    public class ApiController : Controller
    {
        string connectionString = "";

        public IActionResult Index(Product model)
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM Products").ToList();

            return Json(products);
        }

        [HttpPost]
        public IActionResult Add(Product model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    msg = "Eksik veya hatalı bilgi girişi yaptın"
                });

            }

            using var connection = new SqlConnection(connectionString);
            var newRecordId = connection.ExecuteScalar<int>("Insert Into Products (Name, Image, Price, Stock,Created,CategoryId) values (@Name, @Image, @Price,@Stock, @Created,@CategoryId) SELECT SCOPE_IDENTITY()", model);

            model.Id = newRecordId;
            return Ok(new { msg = "Ürün Eklendi." });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Products WHERE Id = @Id";
            var rowsAffected = connection.Execute(sql, new { Id = id });

            return Ok(new { msg = "Ürün silindi." });
        }

        [HttpPost]
        public IActionResult Edit(Product model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    msg = "Eksik veya hatalı bilgi girişi yaptın"
                });

            }
            using var connection = new SqlConnection(connectionString);

            var sql = "Update Products Set Name = @Name, Image = @Image, Price = @Price, Stock = @Stock, Created = @Created, CategoryId = @CategoryId Where Id = @Id";

            var product = new
            {
                Id = model.Id,
                Name = model.Name,
                Image = model.Image,
                Price = model.Price,
                Stock = model.Stock,
                Created = model.Created,
                CategoryId = model.CategoryId
            };

            var affectedRow = connection.Execute(sql, product);

            return Ok(new { msg = "Ürün Güncellendi." });
        }
    }
}
