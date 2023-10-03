using System.Collections.Generic;
using TicketBuddy.Models;
using MongoDB.Driver;
using TicketBuddy.Data;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace TicketBuddy.Data
{
    public class TicketService
    {
        private readonly IMongoCollection<Ticket> _tickets;
        private readonly IMongoCollection<Comment> _comments;

        public TicketService(IOptions<TicketDatabaseSettings> settings)
        {
            // MongoDB bağlantısı ve veritabanı erişimi yapılır
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            // _tickets, "Tickets" koleksiyonunu temsil eden IMongoCollection<Ticket> örneğini alır
            _tickets = database.GetCollection<Ticket>(settings.Value.TicketsCollectionName);

            // _comments, "Comments" koleksiyonunu temsil eden IMongoCollection<Comment> örneğini alır
            _comments = database.GetCollection<Comment>(settings.Value.CommentsCollectionName);
        }

        // Ticket işlemleri

        // Belirli bir kullanıcı adına ait biletleri getirir
        public List<Ticket> GetTicketsByUsername(string username)
        {
            return _tickets.Find(ticket => ticket.Username == username).ToList();
        }

        // Belirli bir kullanıcı adına ve bilet kimliğine ait bilet ve yorumları getirir
        public Ticket GetTicketByIdAndUsername(string id, string username)
        {
            return _tickets.Find(ticket => ticket.Id == id && ticket.Username == username).FirstOrDefault();
        }

        // Belirli bir bilet kimliğine ait bilet getirir
        public Ticket GetTicketById(string id)
        {
            return _tickets.Find(ticket => ticket.Id == id).FirstOrDefault();
        }

        // Biletin lastactivity_at özelliğini günceller
        public bool UpdateTicketLastActivity(string ticketId)
        {
            var lastComment = _comments.Find(comment => comment.TicketId == ticketId)
                                       .SortByDescending(comment => comment.Created_at)
                                       .FirstOrDefault();

            if (lastComment != null)
            {
                var filter = Builders<Ticket>.Filter.Eq(ticket => ticket.Id, ticketId);
                var update = Builders<Ticket>.Update.Set(ticket => ticket.Lastactivity_at, lastComment.Created_at);
                var result = _tickets.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }

            return false;
        }

        // Belirli bir bilet kimliğine ait yorumları getirir
        public List<Comment> GetCommentsByTicketId(string ticketId)
        {
            return _comments.Find(comment => comment.TicketId == ticketId).ToList();
        }

        // Yeni bir bilet oluşturur
        public Ticket CreateTicket(Ticket ticket)
        {
            ticket.TicketId = GetNextTicketId(); // Bir sonraki TicketId değerini alır
            _tickets.InsertOne(ticket);
            return ticket;
        }

        private int GetNextTicketId()
        {
            var lastTicket = _tickets.Find(Builders<Ticket>.Filter.Empty)
                                     .SortByDescending(ticket => ticket.TicketId)
                                     .Limit(1)
                                     .FirstOrDefault();

            if (lastTicket != null)
            {
                return lastTicket.TicketId + 1;
            }

            // İlk biletin oluşturulduğu durumda
            return 1;
        }

        // Belirli bir biletin durumunu günceller
        public bool UpdateTicketStatus(string id, StatusType status)
        {
            if (status != StatusType.Resolved && status != StatusType.Open && status != StatusType.Merged)
            {
                return false; // Geçersiz durum değeri
            }

            var filter = Builders<Ticket>.Filter.Eq("_id", ObjectId.Parse(id));
            var update = Builders<Ticket>.Update.Set("Status", status);

            var updateResult = _tickets.UpdateOne(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return false; // Güncelleme başarısız
            }

            return true; // Başarılı güncelleme
        }



        // Comment işlemleri

        // Belirli bir biletin altına yorum ekler
        public Comment AddComment(Comment comment)
        {
            _comments.InsertOne(comment);
            return comment;
        }
    }
}
