 using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SatisApp.Models;

namespace SatisApp.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "";
        public IActionResult Index(string? MessageCssClass, string? Message)
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT P.*,C.[Name] as 'CategoryName' FROM Products AS P INNER JOIN Categories AS C ON C.Id = P.CategoryId").ToList();

            ViewData["Nickname"] = HttpContext.Session.GetString("Nickname"); 
            ViewBag.Message = Message;
            ViewBag.MessageCssClass = MessageCssClass;

            return View(products);
        }

        public bool CheckLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Nickname")))
            {
                return false;
            }

            return true;
        }

        //public Register GetMail(string email)
        //{
        //    using var connection = new SqlConnection(connectionString);
        //    var sql = "SELECT * FROM Users WHERE Mail = @Mail";
        //    return connection.QueryFirstOrDefault<Register>(sql, new { Mail = email });
        //}
        public int? KullaniciGetir(string nickname)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT Id FROM Users WHERE Nickname = @Nickname";
            var userId = connection.QueryFirstOrDefault<int?>(sql, new { Nickname = nickname });
            return userId;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register(Register? model)
        {
            if (model == null)
            {
                model = new Register();
            }

            return View(model);
        }

        [HttpPost]
        [Route("/Login")]
        public IActionResult Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AuthError"] = "Form eksik.";
                return RedirectToAction("Login");
            }

            model.Password = Helper.Hash(model.Password);
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM Users WHERE Nickname = @Nickname AND Password = @Password";
            var user = connection.QueryFirstOrDefault<Login>(sql, new { model.Nickname, model.Password });

            if (user != null)
            {
                //HttpContext.Session.SetInt32("userId", user.Id);
                HttpContext.Session.SetString("Nickname", user.Nickname);
                ViewData["Nickname"] = HttpContext.Session.GetString("Nickname");

                ViewBag.Message = "login Başarılı";
                return View("Message");
            }

            TempData["AuthError"] = "Kullanıcı adı veya şifre hatalı";
            return View("Login");
        }

        [HttpPost]
        [Route("/KayitOl")]
        public IActionResult KayitOl(Register model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AuthError"] = "Form eksik veya hatalı.";
                return View("Register");
            }

            if (model.Password != model.Pwconfirmend)
            {
                TempData["AuthError"] = "Şifreler Uyuşmuyor.";
                return View("Register", model);
            }

            using (var control = new SqlConnection(connectionString))
            {
                var cntrl = "SELECT * FROM Users WHERE Nickname = @Nickname";
                var user = control.QueryFirstOrDefault(cntrl, new { model.Nickname });
                if (user != null)
                {
                    TempData["AuthError"] = "Bu kullanıcı adı mevcut!.";
                    return View("Register", model);
                }
            }

            model.Created = DateTime.Now;
            model.Password = Helper.Hash(model.Password);

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "INSERT INTO Users (Nickname, Password, Mail, Created) VALUES (@Nickname, @Password, @Mail, @Created)";
                var data = new
                {
                    model.Nickname,
                    model.Password,
                    model.Mail,
                    model.Created,
                };

                try
                {
                    connection.Execute(sql, data);
                    ViewBag.Message = "Kayıt Başarılı";
                }
                catch (Exception ex)
                {
                    TempData["AuthError"] = "Bir hata oluştu: " + ex.Message;
                    return View("Register", model);
                }
            }

            return View("Message");
        }


        [Route("/profil/{nickname}")]
        public IActionResult Profile(string nickname)
        {
            ViewData["Nickname"] = HttpContext.Session.GetString("Nickname"); 
            int? userId = KullaniciGetir(nickname);
            if (userId == null)
            {
                ViewBag.Message = "Böyle bir kullanıcı yok!";
                return View("Message");
            }

            var profil = new Profile();
            using (var connection = new SqlConnection(connectionString))
            {
                if (userId == HttpContext.Session.GetInt32("userId"))
                {
                    ViewBag.profile = true;
                    var sql =
                        "SELECT Comments.Id , users.Nickname , Created FROM Comments LEFT JOIN users on Comments.UserId = users.Id WHERE UserId = @userId ORDER BY Created DESC";
                    var Comments = connection.Query<Comment>(sql, new { UserId = userId }).ToList();
                    profil.Comments = Comments;
                }
                else
                {
                    ViewBag.profile = false;
                    var sql =
                        "SELECT users.Nickname as Nickname, Created FROM Comments LEFT JOIN users on Comments.UserId = users.Id WHERE UserId = @userId ORDER BY Created DESC";

                    var Comments = connection.Query<Comment>(sql, new { UserId = userId }).ToList();
                    profil.Comments = Comments;
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM Users WHERE Id = @userId";
                var profile = connection.QueryFirstOrDefault<Register>(sql, new { UserId = userId });
                profil.User = profile;
            }


            return View(profil);
        }

        public IActionResult Cikis()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult SepeteEkle(int Id)
        {
            using var connection = new SqlConnection(connectionString);

            var urun = connection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id });

            if (urun == null)
            {
                return RedirectToAction("Index", new { MessageCssClass = "alert-danger", Message = "ürün bulunamadı." });
            }

            if (urun.Stock > 0)
            {
                urun.Stock--;

                var sqlstok = "UPDATE Products SET Stock = @Stock WHERE Id = @Id";
                connection.Execute(sqlstok, new { Stock = urun.Stock, Id });

                var sqlinsert = "INSERT INTO Basket (UrunId, UrunAdi, UrunFiyati, UrunAdedi, UrunFoto) VALUES (@UrunId, @UrunAdi, @UrunFiyati, @UrunAdedi, @UrunFoto)";
                var data = new
                {
                    UrunId = Id,
                    UrunAdi = urun.Name,
                    UrunFiyati = urun.Price,
                    UrunAdedi = 1,
                    UrunFoto = urun.Image
                };

                connection.Execute(sqlinsert, data);
                return RedirectToAction("Index", new { MessageCssClass = "alert-success", Message = $"1 adet {urun.Name} sepete eklendi." });
            }

            return RedirectToAction("Index", new { MessageCssClass = "alert-danger", Message = $"{urun.Name} ürünün stok durumu bulunamadı." });
        }

        public IActionResult SatinAl()
        {
            using var connection = new SqlConnection(connectionString);

            var sepet = connection.Query<Sepet>("Select * From Basket").ToList();

            if (sepet.Count > 0)
            {
                foreach (var item in sepet)
                {
                    var Sqlinsert = "Insert Into Sales (UrunId, SatisFiyati, SatisAdedi, UrunAdi) values (@UrunId, @SatisFiyati, @SatisAdedi, @UrunAdi)";

                    var data = new
                    {
                        UrunId = item.UrunId,
                        SatisFiyati = item.UrunFiyati,
                        SatisAdedi = item.UrunAdedi,
                        UrunAdi = item.UrunAdi
                    };
                    connection.Execute(Sqlinsert, data);
                }

                connection.Execute("DELETE From Basket");

                return RedirectToAction("Comment", new { UrunId = sepet.First().UrunId });
            }

            ViewBag.MessageCssClass = "alert-danger";
            ViewBag.Message = "Sepetinizde Hiç Ürün Yok.";

            return View("Message");
        }

        public IActionResult Comment(int UrunId)
        {
            var model = new Comment
            {
                UrunId = UrunId
            };

            return View(model);
        }

        [HttpPost]
        [Route("/addyorum")]
        public IActionResult AddYorum(Comment model)
        {
            model.CreatedTime = DateTime.Now;
            //model.userıd = (int)httpcontext.session.getınt32("userıd");

            using var connection = new SqlConnection(connectionString);
            var sql =
                "INSERT INTO Comments (Summary, CreatedTime, UserId, UrunId) VALUES (@Summary, @CreatedTime, @UserId, @UrunId)";

            var data = new
            {
                Summary = model.Summary,
                CreatedTime = model.CreatedTime,
                UserId = model.UserId,
                UrunId = model.UrunId
            };

            connection.Execute(sql, data);

            return RedirectToAction("Index", new { MessageCssClass = "alert-success", Message = "Yorumunuz başarıyla eklendi!" });
        }
    }
}
