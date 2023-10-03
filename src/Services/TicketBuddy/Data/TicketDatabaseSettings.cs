namespace TicketBuddy.Data
{
    public class TicketDatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string TicketsCollectionName { get; set; } = string.Empty;
        public string CommentsCollectionName { get; set; } = string.Empty;
    }
}
