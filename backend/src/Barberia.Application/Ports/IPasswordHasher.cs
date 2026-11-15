namespace Barberia.Application.Ports;

/// <summary>Puerto que protege las contraseñas y las verifica.</summary>
public interface IPasswordHasher
{
    /// <summary>Devuelve la contraseña protegida, lista para guardar.</summary>
    string Hash(string password);

    /// <summary>Indica si la contraseña coincide con la protegida.</summary>
    bool Verify(string passwordHash, string password);
}
