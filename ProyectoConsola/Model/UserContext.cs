using Microsoft.EntityFrameworkCore;
namespace ProyectoConsola.Model;

public class UserContext
{
    private readonly string _filePath;

    public UserContext(string filePath)
    {
        _filePath = filePath;
    }

    public User FindUser(string userName)
    {
        string[] lines = File.ReadAllLines(_filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts[0] == userName)
            {
                return new User { UserName = parts[0], Clave = parts[1] };
            }
        }
        return null;
    }

    public void AddUser(User user)
    {
        using (StreamWriter writer = File.AppendText(_filePath))
        {
            writer.WriteLine(user.UserName + "," + user.Clave);
        }
    }
}