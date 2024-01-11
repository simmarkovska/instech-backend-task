namespace Claims.Services.Interfaces
{
#pragma warning disable 1591 // Disable warning related to missing XML comments

    public interface IMessageService
    {
        Task SendMessage(dynamic obj);
    }
}
