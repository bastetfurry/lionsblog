using System.Globalization;
using Microsoft.Data.Sqlite;

namespace LionsBlog;

public class Posts
{
    public Posts()
    {
    }

    public int GetPostCount(bool onlyActive=true)
    {
        var sqlcmdstring = "";
        if(onlyActive)
        {
            sqlcmdstring = "select count(id) from posts where isactive = 1;";
        }
        else
        {
            sqlcmdstring = "select count(id) from posts;";
        }
        var database = new Database();

        using var connection = database.getConnection();
        var cmd = new SqliteCommand(sqlcmdstring, connection);

        var reader = cmd.ExecuteReader();
        reader.Read();
        var debugtemp = reader.GetInt32(0);
        return debugtemp;
    }

    public List<PostStruct> GetPosts(int from, int amount, bool onlyActive=true)
    {
        var posts = new List<PostStruct> { };

        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"select
                                            l.id,
                                            l.topic,
                                            l.post,
                                            r.screenname,
                                            l.author,
                                            l.published,
                                            l.lastedit,
                                            l.isactive
                                        from posts l
                                            inner join users r on
                                                r.id = l.author
                                            where l.isactive > $active order by l.id desc
                                            limit $amount offset $from"
                        , connection);
        cmd.Parameters.AddWithValue("$from", from);
        cmd.Parameters.AddWithValue("$amount", amount);
        cmd.Parameters.AddWithValue("$active", onlyActive?0:-1);

        var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            posts.Add(new PostStruct
            {
                Id = reader.GetInt32(0),
                Topic = reader.GetString(1),
                Post = reader.GetString(2),
                Author = reader.GetString(3),
                AuthorId = reader.GetInt32(4),
                Published = reader.GetDateTime(5),
                LastEdit = reader.GetDateTime(6),
                IsActive = reader.GetInt32(7)
            });
        }

        return posts;
    }

    public PostStruct GetSinglePost(int postid)
    {
        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"select
                                            l.id,
                                            l.topic,
                                            l.post,
                                            r.screenname,
                                            l.author,
                                            l.published,
                                            l.lastedit,
                                            l.isactive,
                                            l.tags
                                        from posts l
                                            inner join users r on
                                                r.id = l.author
                                            where l.id = $postid"
                        , connection);
        cmd.Parameters.AddWithValue("$postid", postid);

        var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new PostStruct
            {
                Id = reader.GetInt32(0),
                Topic = reader.GetString(1),
                Post = reader.GetString(2),
                Author = reader.GetString(3),
                AuthorId = reader.GetInt32(4),
                Published = reader.GetDateTime(5),
                LastEdit = reader.GetDateTime(6),
                IsActive = reader.GetInt32(7),
                Tags = reader.GetString(8)
            };
        }

        return null;
    }

    public PostStruct AddOrEditPost(PostStruct post)
    {
        var database = new Database();
        int newpostid;
        using(var connection = database.getConnection())
        {
            var cmdtext = "";
            if (post.Id < 1)
            {
                cmdtext = @"insert into posts
                            (
                                topic,
                                post,
                                author,
                                published,
                                lastedit,
                                isactive,
                                tags
                            )
                            values
                            (
                                $topic,
                                $post,
                                $author,
                                $published,
                                $lastedit,
                                $isactive,
                                $tags
                            )
                            returning id;";
            }
            else
            {
                cmdtext = @"update posts
                            set
                                topic=$topic,
                                post=$post,
                                author=$author,
                                published=$published,
                                lastedit=$lastedit,
                                isactive=$isactive,
                                tags=$tags
                            where
                                id=$id
                            returning id;";
            }


            var cmd = new SqliteCommand(cmdtext, connection);
            if (post.Id > 0) cmd.Parameters.AddWithValue("$id", post.Id);
            cmd.Parameters.AddWithValue("$topic", post.Topic);
            cmd.Parameters.AddWithValue("$post", post.Post);
            cmd.Parameters.AddWithValue("$author", post.AuthorId);
            cmd.Parameters.AddWithValue("$published", post.Published);
            cmd.Parameters.AddWithValue("$lastedit", post.LastEdit);
            cmd.Parameters.AddWithValue("$isactive", post.IsActive);
            cmd.Parameters.AddWithValue("$tags", post.Tags);
            var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                // Should never happen, if it does i hope you have a backup of your DB o.o
                throw new SqliteException("Could not create or update a post!",1);
            }

            newpostid = reader.GetInt32(0);
        }

        return GetSinglePost(newpostid); // Returning the created or updated post.
    }
}
