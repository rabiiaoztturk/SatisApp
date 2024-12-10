using System.ComponentModel.DataAnnotations;

namespace SatisApp.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public int Stock { get; set; }

        public string? Image { get; set; }
        public DateTime Created { get; set; }
        public IFormFile? ImgFile { get; set; }
        public List<Comment>? Comments { get; set; }


    }
}
