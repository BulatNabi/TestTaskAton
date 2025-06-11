using TestTaskAton.Models;
namespace TestTaskAton.Interfaces;

public interface ITokenInterface
{
    Task<string> CreateToken(User user);
}