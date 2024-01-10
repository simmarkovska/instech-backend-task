namespace Claims.Services.Interfaces
{
    public interface IMessageService
    {
        Task SendMessage(dynamic obj);
    }
}
