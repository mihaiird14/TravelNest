namespace TravelNest.ViewModels
{
    public class FriendsMapViewModel
    {
        public List<FriendMarkerDto> Friends { get; set; } = new();
    }

    public class FriendMarkerDto
    {
        public int ProfilId { get; set; }
        public string Nume { get; set; } = "";
        public string ImagineProfil { get; set; } = "/images/profilDefault.png";
        public List<FriendGroupDto> Grupuri { get; set; } = new();
    }

    public class FriendGroupDto
    {
        public int GrupId { get; set; }
        public string? NumeGrup { get; set; }

        public string Status { get; set; } = "";
     
        public string CodTara { get; set; } = "";

        public FlightDto? ZborAzi { get; set; }
    }

    public class FlightDto
    {
        public string OrasPlecare { get; set; } = "";
        public string OrasSosire { get; set; } = "";
        public string AeroportPlecare { get; set; } = "";
        public string AeroportSosire { get; set; } = "";
        public string NumarZbor { get; set; } = "";
        public DateTime DataPlecare { get; set; }
        public DateTime DataSosire { get; set; }
    }
}