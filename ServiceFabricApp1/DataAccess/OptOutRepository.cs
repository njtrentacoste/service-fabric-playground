using Common;
using Common.Interfaces;
using NLog;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataAccess
{
    public class OptOutRepository : IOptOutRepository
    {
        ILogger logger = LogManager.GetCurrentClassLogger();

        string ConnectionString { get; set; }

        public OptOutRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task AddOptOutAsync(OptOut request)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "INSERT INTO Optout (EmailAddress, CampaignId, OrderId, AddDate) VALUES (@EmailAddress, @CampaignId, @OrderId, @AddDate)"
                };

                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@EmailAddress", SqlDbType = System.Data.SqlDbType.NVarChar, Value = request.EmailAddress });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@CampaignId", SqlDbType = System.Data.SqlDbType.VarChar, Value = request.CampaignId });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@OrderId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, Value = request.OrderId });
                cmd.Parameters.Add(new SqlParameter() { ParameterName = "@AddDate", SqlDbType = System.Data.SqlDbType.DateTime, Value = request.AddDate });

                await conn.OpenAsync();

                await cmd.ExecuteNonQueryAsync();

                conn.Close();
            }
        }

        public async Task<List<OptOut>> GetOptOutsAsync()
        {
            var optouts = new List<OptOut>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                var cmd = new SqlCommand()
                {
                    Connection = conn,
                    CommandType = System.Data.CommandType.Text,
                    CommandText = "SELECT * FROM Optout"
                };

                await conn.OpenAsync();

                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    optouts.Add(BuildOptOut(reader));
                }

                reader.Close();
                conn.Close();
            }

            return optouts;
        }

        OptOut BuildOptOut(SqlDataReader reader)
        {
            var optout = new OptOut();

            if (!reader.IsDBNull((int)Common.TableLayouts.OptOut.Id))
            {
                optout.Id = reader.GetInt32((int)Common.TableLayouts.OptOut.Id);
            }

            if (!reader.IsDBNull((int)Common.TableLayouts.OptOut.EmailAddress))
            {
                optout.EmailAddress = reader.GetString((int)Common.TableLayouts.OptOut.EmailAddress);
            }

            if (!reader.IsDBNull((int)Common.TableLayouts.OptOut.CampaignId))
            {
                optout.CampaignId = reader.GetString((int)Common.TableLayouts.OptOut.CampaignId);
            }

            if (!reader.IsDBNull((int)Common.TableLayouts.OptOut.OrderId))
            {
                optout.OrderId = reader.GetGuid((int)Common.TableLayouts.OptOut.OrderId);
            }

            if (!reader.IsDBNull((int)Common.TableLayouts.OptOut.AddDate))
            {
                optout.AddDate = reader.GetDateTime((int)Common.TableLayouts.OptOut.AddDate);
            }

            return optout;
        }
    }
}