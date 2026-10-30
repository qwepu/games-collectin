namespace GameCollection;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK) break;
            using var main = new MainForm();
            if (main.ShowDialog() != DialogResult.Retry) break;   // Retry = «Выйти из аккаунта»
        }
    }
}
