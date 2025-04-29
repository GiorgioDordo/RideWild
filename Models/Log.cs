namespace RideWild.Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }
        public string Application { get; set; }
    }
}
