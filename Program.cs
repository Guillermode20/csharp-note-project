// See https://aka.ms/new-console-template for more information
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
    string notetitle = Console.ReadLine();
    DateTime datetime = DateTime.Now;
    Console.WriteLine($"Note created on: {datetime}");
    Console.WriteLine("Please Enter Note Content");
}

else if (userinput == "2")
{
    Console.WriteLine("View Notes");
}

else if (userinput == "3")
{
    Console.WriteLine("Exit");
    Environment.Exit(0);
}