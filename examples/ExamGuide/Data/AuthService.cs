namespace ExamGuide.Data;
public sealed class AuthService(UserRepository users)
{
    public AppUser? Current { get; private set; }
    public async Task LoginAsync(string login, string password, Puzzle puzzle)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Заполните обязательные поля: логин и пароль");
        var user = await users.FindAsync(login);
        if (user?.IsBlocked == true) throw new ArgumentException("Вы заблокированы. Обратитесь к администратору");
        var solved = puzzle.IsSolved;
        puzzle.Shuffle(); // Пазл используется для одной отправки, даже при ошибке БД.
        if (user is null || !solved || !users.Verify(user, password))
        {
            if (user is not null)
            {
                await users.RegisterFailureAsync(user.Id);
                if ((await users.FindAsync(login))?.IsBlocked == true)
                    throw new ArgumentException("Вы заблокированы. Обратитесь к администратору");
            }
            throw new ArgumentException(!solved ? "Пазл собран неверно. Расставьте фрагменты и повторите вход" :
                "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
        }
        await users.ResetFailuresAsync(user.Id);
        Current = (await users.AllAsync()).Single(u => u.Id == user.Id);
    }
    public async Task<AppUser> RequireUserAsync(bool administrator = false)
    {
        var current = Current;
        var fresh = current is null ? null : (await users.AllAsync()).FirstOrDefault(u => u.Id == current.Id);
        if (fresh is null || fresh.IsBlocked) { Current = null; throw new UnauthorizedAccessException("Вход недействителен. Войдите заново"); }
        Current = fresh;
        if (administrator && fresh.Role != "Администратор") throw new UnauthorizedAccessException("Доступ только для администратора");
        return fresh;
    }
    public void Logout() => Current = null;
}
