using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketBuddy.Models
{
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Biletin benzersiz kimliğini temsil eder.
        public int TicketId { get; set; }
        public string Username { get; set; } // Kullanıcı adını temsil eder
        public string Reqtype { get; set; } // Talep türünü temsil eder
        public string Title { get; set; } // Başlığı temsil eder
        public string Content { get; set; } // İçeriği temsil eder

        public string Status { get; set; } = "Open"; // Durumu temsil eder, varsayılan olarak "Open" olarak ayarlanır

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Lastactivity_at { get; set; } // Son etkinlik tarihini temsil eder
        public DateTime Created_at { get; set; } // Oluşturma tarihini temsil eder

        public Ticket()
        {
            Lastactivity_at = DateTime.Now;
            Created_at = DateTime.Now;
        }
    }

    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? TicketId { get; set; } // İlgili biletin kimliğini temsil eder

        public string CommentUsername { get; set; } // Yorum yapan kullanıcı adını temsil eder
        public string CommentContent { get; set; } // Yorum içeriğini temsil eder

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Created_at { get; set; } // Oluşturma tarihini temsil eder

        public Comment()
        {
            Created_at = DateTime.Now;
        }
    }

    public class UpdateStatusRequest
    {
        public string Id { get; set; }
        public StatusType Status { get; set; }
    }

    public enum StatusType
    {
        Resolved,
        Open,
        Merged
    }

}