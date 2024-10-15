using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    private static readonly string connectionString = $"Data Source=notes.db;Version=3;";

    static void Main(string[] args)
    {
        // Create SQLite database function
        InitialiseDatabase();
        ShowMainMenu();
    }

    static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("Welcome to Note Taker!");
        // Start screen
        Console.WriteLine("Enter 1 to create a New Note!");
        Console.WriteLine("Enter 2 to View Notes!");
        Console.WriteLine("Enter 3 to Exit!");

        // User input
        string userinput = Console.ReadLine() ?? string.Empty;
        Console.Clear();

        if (userinput == "1") // new note 
        {
            CreateNewNote();
        }
        else if (userinput == "2") // view note 
        {
            ViewNotes();
        }
        else if (userinput == "3") // exit 
        {
            Console.WriteLine("Exit");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("Invalid input. Please try again.");
            ShowMainMenu();
        }
    }

    static void CreateNewNote()
    {
        Console.WriteLine("New Note");
        Console.WriteLine("Please Enter Note Title");
        string noteTitle = Console.ReadLine() ?? string.Empty;
        Console.Clear();

        if (NoteExists(noteTitle))
        {
            Console.WriteLine("Note with the same title already exists. Please try again.");
            Console.ReadKey();
            Console.Clear();
            CreateNewNote();
            return;
        }

        DateTime createdAt = DateTime.Now;
        Console.WriteLine($"Note created on: {createdAt}");
        Console.WriteLine("Please Enter Note Content");
        string noteContent = Console.ReadLine() ?? string.Empty;
        Console.Clear();

        // Insert the note into the database
        CreateNote(noteTitle, noteContent, createdAt);
        ShowMainMenu();
    }

    static bool NoteExists(string noteTitle)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string selectNoteQuery = "SELECT COUNT(*) FROM Notes WHERE NoteTitle = @NoteTitle";
            using (var command = new SQLiteCommand(selectNoteQuery, connection))
            {
                command.Parameters.AddWithValue("@NoteTitle", noteTitle);
                long noteCount = (long)command.ExecuteScalar();
                return noteCount > 0;
            }
        }
    }

    static void InitialiseDatabase()
    {
        string databasePath = "notes.db";
        if (!File.Exists(databasePath))
        {
            SQLiteConnection.CreateFile(databasePath);
        }

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            // Enable WAL mode
            using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", connection))
            {
                cmd.ExecuteNonQuery();
            }

            string createTableQuery = "CREATE TABLE IF NOT EXISTS Notes (Id INTEGER PRIMARY KEY AUTOINCREMENT, NoteTitle TEXT, NoteContent TEXT, CreatedAt DATETIME)";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    static int GetLowestAvailableId()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT Id FROM Notes ORDER BY Id";
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    int expectedId = 1;
                    while (reader.Read())
                    {
                        int currentId = reader.GetInt32(0);
                        if (currentId != expectedId)
                        {
                            return expectedId;
                        }
                        expectedId++;
                    }
                    return expectedId;
                }
            }
        }
    }

    static void CreateNote(string noteTitle, string noteContent, DateTime createdAt)
    {
        int noteId = GetLowestAvailableId();
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string insertNoteQuery = "INSERT INTO Notes (Id, NoteTitle, NoteContent, CreatedAt) VALUES (@Id, @NoteTitle, @NoteContent, @CreatedAt)";
            using (var command = new SQLiteCommand(insertNoteQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", noteId);
                command.Parameters.AddWithValue("@NoteTitle", noteTitle);
                command.Parameters.AddWithValue("@NoteContent", noteContent);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Note created successfully!");
    }

    static void ViewNotes()
    {
        SortDatabase();
        // check if database exists, if not, exit program
        string databasePath = "notes.db";
        if (!File.Exists(databasePath))
        {
            Console.WriteLine("No Notes detected, make some!");
            ShowMainMenu();
            return;
        }

        else if (File.Exists(databasePath))
        {
            Console.WriteLine("Notes:");
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectNotesQuery = "SELECT * FROM Notes";
                using (var command = new SQLiteCommand(selectNotesQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string noteTitle = reader.GetString(1);
                            Console.WriteLine($"{id}. {noteTitle}");
                        }
                    }
                }
            }
        }

        Console.WriteLine("Enter the Note ID to view the Note:");
        if (int.TryParse(Console.ReadLine(), out int noteId))
        {
            Console.Clear();
            string selectNoteByIdQuery = "SELECT NoteTitle, NoteContent, CreatedAt FROM Notes WHERE Id = @Id";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(selectNoteByIdQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", noteId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string noteTitle = reader.GetString(0);
                            string noteContent = reader.GetString(1);
                            DateTime createdAt = reader.GetDateTime(2);
                            Console.WriteLine($"Title: {noteTitle}");
                            Console.WriteLine($"Content: {noteContent}");
                            Console.WriteLine($"Created At: {createdAt}");
                            Console.WriteLine($"Press 1 to edit note content");
                            Console.WriteLine($"Press 2 to delete note");
                            Console.WriteLine($"Press 3 to go back to main menu");
                            if (int.TryParse(Console.ReadLine(), out int userChoice))
                            {
                                if (userChoice == 1)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Enter new note content:");
                                    string newNoteContent = Console.ReadLine() ?? string.Empty;
                                    UpdateNoteContent(noteId, newNoteContent);
                                }

                                else if (userChoice == 2)
                                {
                                    DeleteNoteById(noteId);
                                }

                                else
                                {
                                    ShowMainMenu();
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("Note not found.");
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("Invalid ID");
        }

        ShowMainMenu();
    }

    static void UpdateNoteContent(int noteId, string newNoteContent)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string updateNoteQuery = "UPDATE Notes SET NoteContent = @NoteContent WHERE Id = @Id";
            using (var command = new SQLiteCommand(updateNoteQuery, connection))
            {
                command.Parameters.AddWithValue("@NoteContent", newNoteContent);
                command.Parameters.AddWithValue("@Id", noteId);
                command.ExecuteNonQuery();
            }
        }
    }

    static void DeleteNoteById(int noteId)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string deleteNoteQuery = "DELETE FROM Notes WHERE Id = @Id";
            using (var command = new SQLiteCommand(deleteNoteQuery, connection))
            {
                command.Parameters.AddWithValue("@Id", noteId);
                command.ExecuteNonQuery();
            }
        }
    }

    static void SortDatabase() // function that sorts the database with oldest notes listed last
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string sortNotesQuery = "SELECT * FROM Notes ORDER BY CreatedAt ASC";
            using (var command = new SQLiteCommand(sortNotesQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string noteTitle = reader.GetString(1);
                    }
                }
            }
        }
    }
}
