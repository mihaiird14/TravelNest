namespace TravelNest.Models
{
    public enum StatusUrmarire
    {
        Pending, 
        Accepted,  
        Rejected   
    }
    public class Follow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public virtual Profil Follower { get; set; }
        public int FollowedId { get; set; }
        public virtual Profil Followed { get; set; }

        public StatusUrmarire Status { get; set; }
        public DateTime DataCreat { get; set; } = DateTime.Now;
    }
}
