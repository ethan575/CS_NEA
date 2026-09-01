class Program
{
    [STAThread] // neede for file dialogs
    static void Main()
    {
        using var game = new Game(1280, 720, "test");
        game.Run(); // implement camera using view matrix in shader
    }
}