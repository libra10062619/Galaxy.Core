namespace RabbitMQEventBusSample.DTO
{
    public class MemberInfo : Entity<string>
    {
        public string MemberName { get; set; }

        public string Audit { get; set; }
    }
}
