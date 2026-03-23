using Microsoft.EntityFrameworkCore;
using Neo4j.Driver;
using TravelNest.Data;
using TravelNest.Models;

public class GraphService : GrafInterfata
{
    private readonly ApplicationDbContext _context;
    private readonly IDriver _driver;

    public GraphService(ApplicationDbContext context, IDriver driver)
    {
        _context = context;
        _driver = driver;
    }
    public async Task InitializareGraf()
    {
        //adaugam automat in bd doar daca nu are date in ea
        await using var session = _driver.AsyncSession();
        var dateInBD = await session.ExecuteReadAsync(async tx => {
            var cursor = await tx.RunAsync("MATCH (n) RETURN count(n) as nr");
            var record = await cursor.SingleAsync();
            return record["nr"].As<int>();
        });
        if (dateInBD > 0)
        {
            Console.WriteLine("--> Test BAza de date graf este populata!");
            return;
        }
        Console.WriteLine("--> Test baza de date nu este populata!");
        var listaprofil = await _context.Profils.Include(p => p.User).ToListAsync();
        foreach (var p in listaprofil)
        {
            await SincronizareUseri(p.Id, p.User.UserName);
        }
        // sincronizare follow
        var urm = await _context.Follows.Where(f => f.Status == StatusUrmarire.Accepted).ToListAsync();
        foreach (var f in urm)
        {
            await SincronizareFollow(f.FollowerId, f.FollowedId);
        }
        // sincronizare postări
        var postari = await _context.Postares.ToListAsync();
        foreach (var p in postari)
        {
            await SincronizarePostari(p.Id, p.CreatorId, p.MetaDate);
        }
    }
    public async Task SincronizareUseri(int userId, string username)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            "MERGE (u:Utilizator {id: $id}) SET u.username = $un",
            new { id = userId, un = username }));
    }

    public async Task SincronizareFollow(int followerId, int followedId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            @"MATCH (a:Utilizator {id: $fid}), (b:Utilizator {id: $tid}) 
              MERGE (a)-[:FOLLOWS]->(b)",
            new { fid = followerId, tid = followedId }));
    }

    public async Task SincronizareLike(int userId, int postId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            @"MATCH (u:Utilizator {id: $uid}), (p:Postare {id: $pid}) 
              MERGE (u)-[:INTERESTED_IN]->(p)",
            new { uid = userId, pid = postId }));
    }

    public async Task SincronizarePostari(int postId, int creatorId, string tags)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => {
            await tx.RunAsync(
                @"MATCH (u:Utilizator {id: $uid}) 
                  MERGE (p:Postare {id: $pid}) 
                  MERGE (u)-[:A_POSTAT]->(p)",
                new { uid = creatorId, pid = postId });

            if (!string.IsNullOrEmpty(tags))
            {
                var tagArray = tags.Split(", ");
                foreach (var tag in tagArray)
                {
                    await tx.RunAsync(
                        @"MATCH (p:Postare {id: $pid}) 
                          MERGE (t:Tag {nume: $tname}) 
                          MERGE (p)-[:ARE_TAG]->(t)",
                        new { pid = postId, tname = tag.Trim() });
                }
            }
        });
    }
    public async Task SincronizareStergeLike(int userId, int postId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            @"MATCH (u:Utilizator {id: $uid})-[r:INTERESTED_IN]->(p:Postare {id: $pid}) 
          DELETE r",
            new { uid = userId, pid = postId }));
    }
    public async Task SincronizareStergeFollow(int followerId, int followedId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            "MATCH (a:Utilizator {id: $fid})-[r:FOLLOWS]->(b:Utilizator {id: $tid}) DELETE r",
            new { fid = followerId, tid = followedId }));
    }
    public async Task SincronizareVizualizare(int vizitatorId, int profilVizitatId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            @"MATCH (a:Utilizator {id: $vid}), (b:Utilizator {id: $pid}) 
          MERGE (a)-[r:A_VIZITAT]->(b)
          SET r.ultimaVizita = datetime()",
            new { vid = vizitatorId, pid = profilVizitatId }));
    }
    public async Task SincronizareUtilizator(int userId)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx => await tx.RunAsync(
            "MERGE (u:Utilizator {id: $uid})",
            new { uid = userId }));
    }
    public async Task<List<int>> ConturiSugerate(int userId)
    {
        // 1 - cont dupa vizualizare profil
        // 2 - conturi metaTags (din like uri si postari)
        // 2 - conturi grupuri comune
        // 1 - cont friend of friend
        // daca nu gasim 6, conturi random pe care userul nu le urmareste
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx => {
            var query = @"
                MATCH (me:Utilizator {id: $uid})
                CALL {
                    WITH me
                    MATCH (me)-[r:A_VIZITAT]->(p:Utilizator)
                    WHERE NOT (me)-[:FOLLOWS]->(p) AND p <> me
                    RETURN p.id AS id, 100 AS scor
            
                    UNION
            
                    WITH me
                    MATCH (me)-[:INTERESTED_IN|A_POSTAT]->(post)-[:ARE_TAG]->(t)<-[:ARE_TAG]-(p2)-[:A_POSTAT]-(potential:Utilizator)
                    WHERE NOT (me)-[:FOLLOWS]->(potential) AND potential <> me
                    RETURN potential.id AS id, 50 AS scor
            
                    UNION
            
                    WITH me
                    MATCH (me)-[:MEMBRU_IN]->(g)<-[:MEMBRU_IN]-(potential:Utilizator)
                    WHERE NOT (me)-[:FOLLOWS]->(potential) AND potential <> me
                    RETURN potential.id AS id, 40 AS scor
            
                    UNION
            
                    WITH me
                    MATCH (me)-[:FOLLOWS]->(f)-[:FOLLOWS]->(potential:Utilizator)
                    WHERE NOT (me)-[:FOLLOWS]->(potential) AND potential <> me
                    RETURN potential.id AS id, 30 AS scor

                    UNION

                    WITH me
                    MATCH (potential:Utilizator)
                    WHERE NOT (me)-[:FOLLOWS]->(potential) AND potential <> me
                    RETURN potential.id AS id, 0 AS scor
                }
                WITH id, max(scor) AS scorFinal
                RETURN id
                ORDER BY rand()
                LIMIT 6";

            var rez = await tx.RunAsync(query, new { uid = userId });
            var idConturi = new List<int>();
            while (await rez.FetchAsync())
            {
                idConturi.Add(rez.Current["id"].As<int>());
            }
            return idConturi;
        });
    }
}