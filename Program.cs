using System;
using System.Data.SQLite;
using System.IO;

class Program
{
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
        string databasePath = "notes.db";
        string connectionString = $"Data Source={databasePath};Version=3;";
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
            string connectionString = $"Data Source={databasePath};Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = "CREATE TABLE IF NOT EXISTS Notes (Id INTEGER PRIMARY KEY AUTOINCREMENT, NoteTitle TEXT, NoteContent TEXT, CreatedAt DATETIME)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    static void CreateNote(string noteTitle, string noteContent, DateTime createdAt)
    {
        string databasePath = "notes.db";
        string connectionString = $"Data Source={databasePath};Version=3;";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string insertNoteQuery = "INSERT INTO Notes (NoteTitle, NoteContent, CreatedAt) VALUES (@NoteTitle, @NoteContent, @CreatedAt)";
            using (var command = new SQLiteCommand(insertNoteQuery, connection))
            {
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
        // check if database exists, if not, exit program
        string databasePath = "notes.db";
        if (!File.Exists(databasePath))
        {
            Console.WriteLine("No Notes detected, make some!");
            ShowMainMenu();
            return;
        }

        Console.WriteLine("Notes:");
        string connectionString = $"Data Source={databasePath};Version=3;";
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
                            Console.WriteLine("Press any key to return to the main menu...");
                            Console.ReadKey();
                            {
                                Console.Clear();
                                ShowMainMenu();
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
}
