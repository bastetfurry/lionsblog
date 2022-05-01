namespace LionsBlog;
public class PostStruct
{
    public int Id {get; set;}
    public string Topic {get; set;}
    public string Post {get; set;}
    public string Author {get; set;}
    public int AuthorId {get; set;}
    public DateTime Published {get; set;}
    public DateTime LastEdit {get; set;}
    public int IsActive {get; set;}
    public string Tags {get; set;}
}

