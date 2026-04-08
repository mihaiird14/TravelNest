using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TravelNest.Data;
using TravelNest.Models;

namespace TravelNest.Hubs
{
    public class ChatHub:Hub
    {
        private readonly ApplicationDbContext _context;
        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task JoinGrupChat(int idGrup)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Grup_{idGrup}");
        }

        public async Task TrimiteMesajGrup(int expeditorId, int idGrup, string text)
        {
            try
            {
                var profil = await _context.Profils.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == expeditorId);
                string userName = profil?.User.UserName ?? "User";
                string avatarUrl = string.IsNullOrEmpty(profil?.ImagineProfil) ? "/images/default.png" : profil.ImagineProfil;
                var mesajNou = new Mesaj
                {
                    ContinutContent = text,
                    ExpeditorProfilId = expeditorId,
                    TravelGroupId = idGrup,
                    DataTrimite = DateTime.Now
                };

                _context.Mesaje.Add(mesajNou);

                mesajNou.VizualizariMessages.Add(new VizualizareMesaj
                {
                    ProfilId = expeditorId,
                    DataSeen = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await Clients.Group($"Grup_{idGrup}").SendAsync("PrimesteMesajGrup",
                    expeditorId,
                    text,
                    DateTime.Now.ToString("HH:mm"),
                    mesajNou.Id,
                    userName,
                    avatarUrl,
                    mesajNou.DataTrimite);
            }
            catch (Exception ex)
            {
               
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) 
                    Console.WriteLine(ex.InnerException.Message);
                throw new HubException("Eroare la salvarea mesajului pe server.");
            }
        }
        public async Task MesajEdit(int mesajId, string textNou)
        {
            var mesaj = await _context.Mesaje.FindAsync(mesajId);
            if (mesaj == null) return;

            mesaj.ContinutContent = textNou;
            await _context.SaveChangesAsync();

            if (mesaj.TravelGroupId != null)
            {
                await Clients.Group($"Grup_{mesaj.TravelGroupId}").SendAsync("MesajUpdateEditat", mesajId, textNou);
            }
            else
            {
                var chatId = mesaj.ExpeditorProfilId < mesaj.DestinatarProfilId
                    ? $"Privat_{mesaj.ExpeditorProfilId}_{mesaj.DestinatarProfilId}"
                    : $"Privat_{mesaj.DestinatarProfilId}_{mesaj.ExpeditorProfilId}";
                await Clients.Group(chatId).SendAsync("MesajUpdateEditat", mesajId, textNou);
            }
        }

        public async Task MesajDelete(int mesajId)
        {
            var mesaj = await _context.Mesaje.FindAsync(mesajId);
            if (mesaj == null) 
                return;

            if ((DateTime.Now - mesaj.DataTrimite).TotalMinutes > 10)
                throw new HubException("Timpul a expirat!");

            int? grupId = mesaj.TravelGroupId;
            int expId = mesaj.ExpeditorProfilId;
            int? destId = mesaj.DestinatarProfilId;

            _context.Mesaje.Remove(mesaj);
            await _context.SaveChangesAsync();

            if (grupId != null)
            {
                await Clients.Group($"Grup_{grupId}").SendAsync("MesajEliminatSters", mesajId);
            }
            else
            {
                var chatId = expId < destId ? $"Privat_{expId}_{destId}" : $"Privat_{destId}_{expId}";
                await Clients.Group(chatId).SendAsync("MesajEliminatSters", mesajId);
            }
        }
        public async Task JoinPrivateChat(int idEu, int idEl)
        {
            var chatId = idEu < idEl ? $"Privat_{idEu}_{idEl}" : $"Privat_{idEl}_{idEu}";
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task TrimiteMesajPrivat(int expeditorId, int destinatarId, string text)
        {
            var profilExp = await _context.Profils.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == expeditorId);
            string numeExp = profilExp?.User.UserName ?? "User";
            string imgExp = profilExp?.ImagineProfil ?? "/images/default.png";

            var mesajNou = new Mesaj
            {
                ContinutContent = text,
                ExpeditorProfilId = expeditorId,
                DestinatarProfilId = destinatarId,
                DataTrimite = DateTime.Now,
                TravelGroupId = null
            };

            _context.Mesaje.Add(mesajNou);
            await _context.SaveChangesAsync();

            var chatId = expeditorId < destinatarId ? $"Privat_{expeditorId}_{destinatarId}" : $"Privat_{destinatarId}_{expeditorId}";

            await Clients.Group(chatId).SendAsync("PrimesteMesajPrivat", 
        expeditorId, text, DateTime.Now.ToString("HH:mm"), numeExp, imgExp, mesajNou.Id, mesajNou.DataTrimite);
        }
    }
}
