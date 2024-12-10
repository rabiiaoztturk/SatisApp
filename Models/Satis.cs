namespace SatisApp.Models
{
    public class Satis
    {
        public int Id { get; set; }
        public string UrunId { get; set; }
        public int SatisAdedi { get; set; } = 1;
        public int SatisFiyati { get; set; }
        public required string UrunAdi { get; set; }
    }
}
