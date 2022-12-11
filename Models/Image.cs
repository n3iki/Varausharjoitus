namespace Varausharjoitus.Models
{
    public class Image
    {
        public long Id { get; set; }
        public String? Description { get; set; }
        public String Url { get; set; }
        public virtual Item? Target { get; set; }
    }


    public class ImageDTO
    {
        public String Url { get; set; }
        public String? Description { get; set; }
    }

}