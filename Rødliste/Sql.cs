using System.Collections.Generic;
using System.Numerics;
using Npgsql;

namespace Rødliste
{
    internal class Sql
    {
        public List<string> From { get; set; }
        public List<string> Where { get; set; }

        public static void Execute(Regel regel, string connString)
        {
            var sql = CreateSqlStringForRegel(regel.Sql);

            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    regel.Naturområder = new List<BigInteger>();
                    while (reader.Read())
                    {
                        var id = reader.GetInt64(0);
                        regel.Naturområder.Add(id);
                    }
                }

                //// Insert some data
                //using (var cmd = new NpgsqlCommand())
                //{
                //    cmd.Connection = conn;
                //    cmd.CommandText = "INSERT INTO data (some_field) VALUES (@p)";
                //    cmd.Parameters.AddWithValue("p", "Hello world");
                //    cmd.ExecuteNonQuery();
                //}
            }
        }

        private static string CreateSqlStringForRegel(Sql regelSql)
        {
            var sql = "SELECT na.geometry_id FROM " + string.Join(",", regelSql.From);

            sql += " WHERE " + string.Join(" AND ", regelSql.Where);

            return sql;
        }
    }
}