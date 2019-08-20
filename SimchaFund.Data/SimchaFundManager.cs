using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SimchaFund.Data
{
    public class SimchaFundManager
    {
        private string _connectionString;

        public SimchaFundManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Simchas> GetAllSimchas()
        {
            var simchas = new List<Simchas>();
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Simchas";
            connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var simcha = new Simchas();
                simcha.Id = (int)reader["Id"];
                simcha.Name = (string)reader["Name"];
                simcha.Date = (DateTime)reader["Date"];
                simcha.ContributorAmount = (int)reader["ContributorAmount"];
                simcha.Total = (int)reader["Total"];
                simchas.Add(simcha);
            }
            return simchas;            
        }
        public int GetContributorCount()
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Contributors";
            connection.Open();
            return (int)cmd.ExecuteScalar();
        }
        public void AddSimcha(Simchas simcha)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Simchas (Name, Date) 
                                    VALUES (@name, @date) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", simcha.Name);
            cmd.Parameters.AddWithValue("@date", simcha.Date);
            simcha.Id = (int)cmd.ExecuteScalar();
        }
        public IEnumerable<Contributors> GetContributors()
        {
            var contributors = new List<Contributors>();
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = "SELECT * FROM Contributors";
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var contributor = new Contributors();
                contributor.Id = (int)reader["Id"];
                contributor.FirstName = (string)reader["FirstName"];
                contributor.LastName = (string)reader["LastName"];
                contributor.Date = (DateTime)reader["Date"];
                contributor.AlwaysInclude = (bool)reader["AlwaysInclude"];
                contributor.Balance = (int)reader["Balance"];
                contributors.Add(contributor);
            }
            return contributors;
        }
        public void AddContributor(Contributors contributor)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Contributors (FirstName, LastName, Date, 
                                    AlwaysInclude) VALUES (@firstName, @lastName, 
                                    @date, @alwaysInclude) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@date", contributor.Date);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            contributor.Id = (int)cmd.ExecuteScalar();
        }
        public void AddDeposit(Deposits deposit)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"INSERT INTO Deposits (ContributorId, Amount, Date) VALUES 
                                    (@contributorId, @amount, @date) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@firstName", deposit.ContributorId);
            cmd.Parameters.AddWithValue("@lastName", deposit.Amount);
            cmd.Parameters.AddWithValue("@date", deposit.Date);
            cmd.ExecuteNonQuery();
        }
        public void UpdateContributor(Contributors contributor)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = @"UPDATE Contributors SET FirstName = @firstName, 
                                    LastName = @lastName, Date = @date, 
                                    AlwaysInclude = @alwaysInclude WHERE Id = @id";
            cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@date", contributor.Date);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            cmd.ExecuteNonQuery();
        }
        public Simchas GetSimchaById(int simchaId)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = "SELECT * FROM Simchas WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", simchaId);
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }
            var simcha = new Simchas();
            simcha.Id = (int)reader["Id"];
            simcha.Name = (string)reader["Name"];
            simcha.Date = (DateTime)reader["Date"];
            simcha.ContributorAmount = (int)reader["ContributorAmount"];
            simcha.Total = (int)reader["Total"];
            return simcha;
        }
        public IEnumerable<SimchaContributor> GetSimchaContributors(int simchaId)
        {
            IEnumerable<Contributors> contributors = GetContributors();
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = "SELECT * FROM Contributors WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", simchaId);
            var reader = cmd.ExecuteReader();
            List<Contributions> contributions = new List<Contributions>();
            while (reader.Read())
            {
                Contributions contribution = new Contributions
                {
                    Amount = (decimal)reader["Amount"],
                    SimchaId = simchaId,
                    ContributorId = (int)reader["ContributorId"]
                };
                contributions.Add(contribution);
            }

            return contributors.Select(contributor =>
            {
                var sc = new SimchaContributor();
                sc.FirstName = contributor.FirstName;
                sc.LastName = contributor.LastName;
                sc.AlwaysInclude = contributor.AlwaysInclude;
                sc.ContributorId = contributor.Id;
                sc.Balance = contributor.Balance;
                Contributions contribution = contributions.FirstOrDefault(c => c.ContributorId == contributor.Id);
                if (contribution != null)
                {
                    sc.Amount = contribution.Amount;
                }
                return sc;
            });
        }
        public IEnumerable<SimchaContributor> GetSimchaContributorsOneQuery(int simchaId)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT *, 
(
	(SELECT ISNULL(SUM(d.Amount), 0) FROM Deposits d WHERE d.ContributorId = c.Id)
	- 
	(SELECT ISNULL(SUM(co.Amount), 0) FROM Contributions co WHERE co.ContributorId = c.Id)
) as 'Balance', (
	SELECT Amount from Contributions WHERE SimchaId = @simchaId AND ContributorId = c.Id
) as 'Amount' FROM Contributors c";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            var reader = cmd.ExecuteReader();
            List<SimchaContributor> result = new List<SimchaContributor>();
            while (reader.Read())
            {
                var contributor = new SimchaContributor();
                contributor.ContributorId = (int)reader["Id"];
                contributor.FirstName = (string)reader["FirstName"];
                contributor.LastName = (string)reader["LastName"];
                contributor.AlwaysInclude = (bool)reader["AlwaysInclude"];
                contributor.Balance = (decimal)reader["Balance"];
                object value = reader["Amount"];
                if (value != DBNull.Value)
                {
                    contributor.Amount = (decimal)value;
                }
                result.Add(contributor);
            }

            return result;
        }
        public void UpdateSimchaContributions(int simchaId, IEnumerable<ContributionInclusion> contributors)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Contributions WHERE SimchaId = @simchaId";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);

            connection.Open();
            cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
            cmd.CommandText = @"INSERT INTO Contributions (SimchaId, ContributorId, Amount)
                                    VALUES (@simchaId, @contributorId, @amount)";
            foreach (ContributionInclusion contributor in contributors.Where(c => c.Include))
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@simchaId", simchaId);
                cmd.Parameters.AddWithValue("@contributorId", contributor.ContributorId);
                cmd.Parameters.AddWithValue("@amount", contributor.Amount);
                cmd.ExecuteNonQuery();
            }
        }
        public IEnumerable<Deposits> GetDepositsById(int contribId)
        {
            var deposits = new List<Deposits>();
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = "SELECT * FROM Deposits WHERE ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", contribId);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var deposit = new Deposits();
                deposit.Id = (int)reader["Id"];
                deposit.Amount = (int)reader["Amount"];
                deposit.Date = (DateTime)reader["Date"];
                deposits.Add(deposit);
            }
            return deposits;
        }
        public IEnumerable<Contributions> GetContributionsById(int contribId)
        {
            var contributions = new List<Contributions>();
            var connection = new SqlConnection(_connectionString);
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = "SELECT * FROM Contributions WHERE ContributorId = @id";
            cmd.Parameters.AddWithValue("@id", contribId);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var contribution = new Contributions();
                contribution.SimchaId = (int)reader["SimchaId"];
                contribution.SimchaName = (string)reader["SimchaName"];
                contribution.ContributorId = (int)reader["ContributorId"];
                contribution.Amount = (int)reader["Amount"];
                contribution.Date = (DateTime)reader["Date"];
                contributions.Add(contribution);
            }
            return contributions;
        }
    }
}
