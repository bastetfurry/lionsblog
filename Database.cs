using System.Data;
using Microsoft.Data.Sqlite;

namespace LionsBlog;

public class Database
{
    private SqliteConnection _connection;
    private readonly Configuration _configuration;

    public Database()
    {
        _configuration = ConfigurationProvider.GetConfiguration();

        var createTheTables = !File.Exists(Path.Combine(Directory.GetCurrentDirectory(), _configuration.DatabaseFilename));
        _connection = getConnection();
        if (createTheTables)
            createTables();

        SetBehavior();
    }

    ~Database()
    {
        _connection.Close();
    }

    public void SetBehavior()
    {
        using var connection = getConnection();
        var cmd = new SqliteCommand("PRAGMA synchronous = Full", connection);
        cmd.ExecuteNonQuery();
    }

    public SqliteConnection getConnection()
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            _connection = new SqliteConnection($"Data Source={_configuration.DatabaseFilename}; Pooling=True;");
            _connection.Open();
        }
        return _connection;
    }

    private void createTables()
    {
        using var connection = getConnection();

        // TABLE posts
        var cmd = new SqliteCommand(
            @"CREATE TABLE ""posts"" (
                            ""id""	INTEGER,
                            ""topic""	TEXT NOT NULL DEFAULT '',
                            ""post""	TEXT NOT NULL DEFAULT '',
                            ""author""	INT NOT NULL DEFAULT 1,
                            ""published""	TEXT NOT NULL DEFAULT (DATETIME('now')),
                            ""lastedit""	TEXT NOT NULL DEFAULT (DATETIME('now')),
                            ""isactive""	INT NOT NULL DEFAULT 0,
                            ""tags""	TEXT NOT NULL DEFAULT '',
                            PRIMARY KEY(""id"" AUTOINCREMENT)
                        );"
            , connection);
        cmd.ExecuteNonQuery();

        // TABLE users
        cmd = new SqliteCommand(
            @"CREATE TABLE ""users"" (
                            ""id""	            INTEGER,
                            ""name""	        TEXT NOT NULL DEFAULT '',
                        	""screenname""	    TEXT NOT NULL DEFAULT '',
                            ""email""	        TEXT NOT NULL DEFAULT '',
                            ""enabled""	        INT NOT NULL DEFAULT 1,
                            ""passwordhashed""	TEXT NOT NULL DEFAULT '',
                            PRIMARY KEY(""id"")
                        );"
            , connection);
        cmd.ExecuteNonQuery();

        cmd = new SqliteCommand(
            @"CREATE UNIQUE INDEX ""idx_users_name"" ON ""users"" (
	                        ""name""
                        );"
            , connection
        );
        cmd.ExecuteNonQuery();

        // TABLE cookies
        cmd = new SqliteCommand(
            @"CREATE TABLE ""cookies"" (
                            ""cookiekey""	TEXT NOT NULL,
                            ""name""	    TEXT NOT NULL,
                            ""lastused""	TEXT DEFAULT (DATETIME('now')),
                            ""ip""	        TEXT NOT NULL,
                            PRIMARY KEY(""cookiekey"")
                        );"
            , connection);
        cmd.ExecuteNonQuery();

        cmd = new SqliteCommand(
            @"CREATE UNIQUE INDEX ""idx_cookies_token"" ON ""cookies"" (
                            ""cookiekey""
                        );"
            , connection
        );
        cmd.ExecuteNonQuery();

        var users = new Users();
        var user = new UserStruct
        {
            id = 0,
            name = _configuration.DefaultUser,
            screenname = _configuration.DefaultScreenname,
            email = _configuration.DefaultEMail,
            enabled = 1,
            passwordhashed = ""
        };

        users.AddOrEditUser(user, _configuration.DefaultPassword);
    }
}