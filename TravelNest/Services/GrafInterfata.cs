using System.Threading.Tasks;

public interface GrafInterfata
{
    Task InitializareGraf();
    Task SincronizareFollow(int followerId, int followedId);
    Task SincronizareStergeFollow(int followerId, int followedId);
    Task SincronizareLike(int userId, int postId);
    Task SincronizareStergeLike(int userId, int postId);
    Task SincronizareUseri(int userId, string username);
    Task SincronizareUtilizator(int userId);
    Task SincronizarePostari(int postId, int creatorId, string tags);
    Task SincronizareVizualizare(int vizitatorId, int profilVizitatId);
    Task<List<int>> ConturiSugerate(int userId);
}