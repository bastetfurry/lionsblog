using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

namespace LionsBlog;

public class Users
{
    public Users()
    {
    }

    public bool PasswordIsOk(string username, string password)
    {
        var userdata = GetUser(username);
        if (userdata == null)
            return false;

        var hasher = new PasswordHasher<string>();

        return hasher.VerifyHashedPassword(username, userdata.passwordhashed, password) == PasswordVerificationResult.Success;
    }

    public UserStruct AddOrEditUser(UserStruct user, string newpassword = "")
    {
        var database = new Database();
        int newuserid;

        using(var connection = database.getConnection())
        {
            if (!String.IsNullOrEmpty(newpassword))
            {
                var hasher = new PasswordHasher<string>();
                user.passwordhashed = hasher.HashPassword(user.name, newpassword);
            }

            var cmdtext = "";
            if(user.id < 1)
            {
                cmdtext = @"insert into users
                                (
                                    name,
                                    screenname,
                                    email,
                                    enabled,
                                    passwordhashed
                                )
                                values
                                (
                                    $name,
                                    $screenname,
                                    $email,
                                    $enabled,
                                    $passwordhashed
                                ) returning id;";
            }
            else
            {
                cmdtext = @"update users
                            set
                                name=$name,
                                screenname=$screenname,
                                email=$email,
                                enabled=$enabled,
                                passwordhashed=$passwordhashed
                            where
                                id=$id
                            returning id;";
            }


            var cmd = new SqliteCommand(cmdtext, connection);
            if(user.id > 0)
                cmd.Parameters.AddWithValue("$id", user.id);
            cmd.Parameters.AddWithValue("$name", user.name);
            cmd.Parameters.AddWithValue("$screenname", user.screenname);
            cmd.Parameters.AddWithValue("$email", user.email);
            cmd.Parameters.AddWithValue("$enabled", user.enabled);
            cmd.Parameters.AddWithValue("$passwordhashed", user.passwordhashed);
            var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                // Should never happen, if it does i hope you have a backup of your DB o.o
                throw new SqliteException("Could not create or update a user!",1);
            }

            newuserid = reader.GetInt32(0);
        }

        return GetUser("",newuserid);
    }

    public string GetUsernameByTokenFromHttpRequest(HttpRequest request)
    {
        var ip = request.HttpContext.Connection.RemoteIpAddress;
        var ipstring = ip == null ? "unknown" : ip.ToString();
        var token = request.Cookies["lionsblog_token"];

        if (String.IsNullOrEmpty(token))
            return null;

        var cookieKeyContainer = new JwtBuilder()
            .WithAlgorithm(new HMACSHA512Algorithm()) // symmetric
            .WithSecret(ConfigurationProvider.GetConfiguration().TokenSecret)
            .MustVerifySignature()
            .Decode<CookieKeyStruct>(token);

        return CheckCookie(cookieKeyContainer.cookieKey, ipstring);
    }

    public void AddOrUpdateCookie(string name, string cookieKey, string ip)
    {
        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"replace into cookies
                                        (
                                            cookiekey,
                                            name,
                                            lastused,
                                            ip
                                        )
                                        values
                                        (
                                            $cookiekey,
                                            $name,
                                            DATETIME('now'),
                                            $ip
                                        );"
                        , connection);
        
        cmd.Parameters.AddWithValue("$cookiekey", cookieKey);
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$ip", ip);
        cmd.ExecuteNonQuery();
    }

    public string CheckCookie(string cookieKey, string ip)
    {
        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"select
                                            name
                                        from
                                            cookies
                                        where
                                            cookiekey=$cookiekey
                                            and ip=$ip"
                        , connection);
        cmd.Parameters.AddWithValue("$cookiekey", cookieKey);
        cmd.Parameters.AddWithValue("$ip", ip);

        var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        return reader.GetString(0);
    }

    public void DeleteCookie(string cookieKey)
    {
        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"delete from
                                            cookies
                                        where
                                            cookiekey=$cookieKey;"
                        , connection);

        cmd.Parameters.AddWithValue("$cookieKey", cookieKey);
        cmd.ExecuteNonQuery();
    }

    public UserStruct GetUser(string user = "", int id = 0)
    {
        var database = new Database();

        using var connection = database.getConnection();

        var reader = (String.IsNullOrEmpty(user) ? getSqlReaderForUserById(id, connection) : getSqlReaderForUserByName(user, connection));

        if (!reader.Read())
            return null; //User not found

        return new UserStruct
        {
            id = reader.GetInt32(0),
            name = reader.GetString(1),
            screenname = reader.GetString(2),
            email = reader.GetString(3),
            passwordhashed = reader.GetString(4),
            enabled = reader.GetInt32(5)
        };
    }

    public List<UserStruct> GetUsers()
    {
        var returnuserlist = new List<UserStruct>();

        var database = new Database();

        using var connection = database.getConnection();

        var cmd = new SqliteCommand(@"select
                                            id,
                                            name,
                                            screenname,
                                            email,
                                            passwordhashed,
                                            enabled
                                        from
                                            users;"
                        , connection);

        var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            returnuserlist.Add(
                new UserStruct
                {
                    id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    screenname = reader.GetString(2),
                    email = reader.GetString(3),
                    passwordhashed = reader.GetString(4),
                    enabled = reader.GetInt32(5)
                }
            );
        }
        return returnuserlist;
    }

    private SqliteDataReader getSqlReaderForUserByName(string name, SqliteConnection connection)
    {
        var cmd = new SqliteCommand(@"select
                                            id,
                                            name,
                                            screenname,
                                            email,
                                            passwordhashed,
                                            enabled
                                        from
                                            users
                                        where
                                            name=$name;"
                        , connection);
        cmd.Parameters.AddWithValue("$name", name);

        return cmd.ExecuteReader();
    }

    private SqliteDataReader getSqlReaderForUserById(int id, SqliteConnection connection)
    {
        var cmd = new SqliteCommand(@"select
                                            id,
                                            name,
                                            screenname,
                                            email,
                                            passwordhashed,
                                            enabled
                                        from
                                            users
                                        where
                                            id=$id"
                        , connection);
        cmd.Parameters.AddWithValue("$id", id);

        return cmd.ExecuteReader();
    }
}