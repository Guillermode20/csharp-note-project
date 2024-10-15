using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Create SQLite database function
        InitialiseDatabase();
        Console.WriteLine("Welcome to Note Taker!");
        Console.WriteLine("Please enter your name:");
        string username = Console.ReadLine();
        Console.WriteLine($"Hello, {username}!");
        
        // Start screen
        Console.WriteLine("Press 1 to create a New Note!");
        Console.WriteLine("Press 2 to View Notes!");
        Console.WriteLine("Press 3 to Exit!");
        
        // User input
        string userinput = Console.ReadLine();
        if (userinput == "1")
        {
            Console.WriteLine("New Note");
            Console.WriteLine($"Hello, {username}!");
            Console.WriteLine("Please Enter Note Title");
            string noteTitle = Console.ReadLine();
            DateTime createdAt = DateTime.Now;
            Console.WriteLine($"Note created on: {createdAt}");
            Console.WriteLine("Please Enter Note Content");
            string noteContent = Console.ReadLine();
            
            // Insert the note into the database
            CreateNote(noteTitle, noteContent, createdAt);
        }
        else if (userinput == "2")
        {
            ViewNotes();
        }
        else if (userinput == "3")
        {
            Console.WriteLine("Exit");
            Environment.Exit(0);
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
        else if (File.Exists(databasePath))
        {
            // if database exists, don't create a new one
            Console.WriteLine("Database already exists!");
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
            Environment.Exit(0);
        }

        if (File.Exists(databasePath))
        {
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
        }
    }
}
