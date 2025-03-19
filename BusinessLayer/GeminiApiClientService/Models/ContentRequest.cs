namespace BusinessLayer.Models.ContentRequest;

public class ContentRequest
{
    public Content[] contents { get; set; }
}

public class Content
{
    public Part[] parts { get; set; }
}

public class Part
{
    public string text { get; set; }
}

//public class ContentRequest
//{
//    public Content[] Content { get; set; } = [];
//}
//public class Content
//{
//    public Part[] Parts { get; set; } = [];
//}

//public class Part
//{
//    public string Text { get; set; } = string.Empty;
//}
