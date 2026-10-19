namespace RepositoryContracts;

public class EntityNotFoundException : InvalidOperationException
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}
