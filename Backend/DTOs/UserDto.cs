namespace EduSync.Backend.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } // "Student" or "Instructor"
}

public class RegisterUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}

public class LoginUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}