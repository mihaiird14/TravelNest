using Microsoft.AspNetCore.SignalR;
namespace TravelNest.Hubs
{
    public class NotificariHub: Hub
    {
        public async Task TrimiteNotificare(string userId, string titlu, string mesaj, string tip, string expeditor, int idNotificare)
        {
            await Clients.User(userId).SendAsync("PrimesteNotificare", titlu, mesaj, tip, expeditor, idNotificare);
        }
    }
}
