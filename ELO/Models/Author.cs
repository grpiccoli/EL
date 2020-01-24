namespace ELO.Models
{
    public class Author
    {
        public int Id { get; set; }
        public int PublicationId { get; set; }
        public virtual Publication Publication { get; set; }
        public string Last { get; set; }
        public string Name { get; set; }
    }
}
