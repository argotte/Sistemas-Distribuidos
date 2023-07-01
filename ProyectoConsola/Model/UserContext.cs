using System.Net;
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
        if (!File.Exists(_filePath)) File.CreateText(_filePath); 
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
        using (StreamWriter writer = (File.Exists(_filePath) ? File.AppendText(_filePath) : File.CreateText(_filePath)))
        {
            writer.WriteLine(user.UserName + "," + user.Clave);
        }
    }
}