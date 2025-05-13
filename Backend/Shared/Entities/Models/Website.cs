namespace Backend.Shared.Entities.Models
{
    public class Website : BaseEntity
    {
        public string Name { get; set; }
        public string HomeUrl { get; set; }
        public string LoginUrl { get; set; }
        public string OrdersUrl { get; set; }
        public bool IsActive { get; set; }
    }
}