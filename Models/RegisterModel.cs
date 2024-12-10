using System.ComponentModel.DataAnnotations;

namespace SatisApp.Models
{
    public class Register
    {
        public int Id { get; set; }
        [Required]
        public string Nickname { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Pwconfirmend { get; set; }
        [Required]
        public string Mail { get; set; }
        public DateTime Created { get; set; }
    }

    public class Profile
    {
        public List<Comment> Comments { get; set; }
        public Register User { get; set; }
    }
    public class Login
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Nickname { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class ResetPwToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public bool Used { get; set; }
    }

    public class PwReset
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Pw { get; set; }
    }
    public class Comment
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public DateTime CreatedTime { get; set; }
        public int UserId { get; set; }
        public int UrunId { get; set; }
    }
}
