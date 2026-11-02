using System.Data;

namespace SharedKernel.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
