using Microsoft.AspNetCore.Mvc;
using System;
using TicketBuddy.Data;
using TicketBuddy.Models;

namespace TicketBuddy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketController(TicketService ticketService)
        {
            // Bu işlem, TicketService sınıfının bir örneğini alarak _ticketService alanına TicketController'ın bu bağımlılığı kullanmasını sağlar.
            _ticketService = ticketService;
        }

        [HttpGet("{username}")]
        public ActionResult<List<Ticket>> GetTickets(string username)
        {
            // Belirli bir kullanıcı adına ait biletleri getirir
            var tickets = _ticketService.GetTicketsByUsername(username);

            // Biletleri LastActivityAt tarihine göre sırala (en yeni önce)
            tickets = tickets.OrderByDescending(ticket => ticket.Lastactivity_at).ToList();

            return Ok(tickets);
        }

        [HttpGet("{username}/{ticketId}")]
        public ActionResult<Dictionary<string, object>> GetTicket(string username, string ticketId)
        {
            // Belirli bir kullanıcı adına ve bilet kimliğine ait bilet ve yorumları getirir
            var ticket = _ticketService.GetTicketByIdAndUsername(ticketId, username);
            if (ticket == null)
            {
                return NotFound();
            }

            var comments = _ticketService.GetCommentsByTicketId(ticketId);

            var ticketWithComments = new
            {
                ticket = ticket,
                comments = comments
            };

            return Ok(ticketWithComments);
        }


        [HttpPost("request")]
        public ActionResult<Ticket> CreateTicket([FromBody] Ticket request)
        {
            // Yeni bir bilet oluşturur
            var ticket = new Ticket
            {
                Username = request.Username,
                Reqtype = request.Reqtype,
                Title = request.Title,
                Content = request.Content
            };

            _ticketService.CreateTicket(ticket);

            return CreatedAtAction(nameof(GetTicket), new { username = ticket.Username, ticketId = ticket.Id }, ticket);
        }

        [HttpPost("answer/{ticketId}")]
        public ActionResult<Comment> AddCommentToTicket(string ticketId, [FromBody] Comment commentRequest)
        {
            // Belirli bir biletin altına yorum ekler
            var ticket = _ticketService.GetTicketById(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            var comment = new Comment
            {
                TicketId = ticketId,
                CommentUsername = commentRequest.CommentUsername,
                CommentContent = commentRequest.CommentContent,
            };

            _ticketService.AddComment(comment);

            // Biletin lastactivity_at özelliğini günceller
            _ticketService.UpdateTicketLastActivity(ticketId);

            return Ok(comment);
        }


        [HttpPut("updatestatus")]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            if (!_ticketService.UpdateTicketStatus(request.Id, request.Status))
            {
                return NotFound();
            }
            return Ok();
        }

    }
}