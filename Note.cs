using Microsoft.Data.Sqlite;

internal static class NotesDb
{
    private static readonly string ConnectionString = "Data Source=notes.db;";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand();
        command.Connection = connection;
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Notes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
        ";
        command.ExecuteNonQuery();
    }

    public static List<NoteItem> LoadNotes()
    {
        var notes = new List<NoteItem>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("SELECT Id, Title, Content, CreatedAt FROM Notes ORDER BY Id DESC", connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            notes.Add(new NoteItem
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Content = reader.GetString(2),
                CreatedAt = reader.GetString(3)
            });
        }

        return notes;
    }

    public static void SaveNote(string title, string content)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("INSERT INTO Notes (Title, Content) VALUES (@title, @content)", connection);
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@content", content);
        command.ExecuteNonQuery();
    }

    public static void UpdateNote(int id, string title, string content)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("UPDATE Notes SET Title = @title, Content = @content WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@title", title);
        command.Parameters.AddWithValue("@content", content);
        command.ExecuteNonQuery();
    }

    public static void DeleteNote(int id)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = new SqliteCommand("DELETE FROM Notes WHERE Id = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }
}

internal sealed class NoteItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}