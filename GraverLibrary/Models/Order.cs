namespace GraverLibrary.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string BarCodeValue { get; set; }
        public bool IsMarked { get; set; } = false;
    }
}
