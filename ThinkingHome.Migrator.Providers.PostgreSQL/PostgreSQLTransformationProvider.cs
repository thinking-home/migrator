using Npgsql;

namespace ThinkingHome.Migrator.Providers.PostgreSQL
{
    public class PostgreSQLTransformationProvider : TransformationProvider<NpgsqlConnection>
    {
        public PostgreSQLTransformationProvider(NpgsqlConnection connection) : base(connection)
        {
        }
    }
}
