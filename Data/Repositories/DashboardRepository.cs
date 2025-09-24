using System.Collections.Generic;
using System.Data;
using Dapper;
using Core.Dtos;
namespace Data.Repositories
{
    public class DashboardRepository
    {
        public IEnumerable<PlantSummaryDto> LoadSummary(string userName)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<PlantSummaryDto>(
                    "dbo.PCN_Dashboard_Summary",
                    new { UserName = userName },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        public IEnumerable<DeptCatalogItem> LoadDeptCatalog(string plant)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<DeptCatalogItem>(
                    "dbo.PCN_Dashboard_Catalog",
                    new { Plant = plant },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        public IEnumerable<StatusItem> LoadStatusByPlant(string plant)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<StatusItem>(
                    "dbo.PCN_Dashboard_StatusByPlant",
                    new { Plant = plant },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
        public IEnumerable<UserRight> LoadRights(string userId, string plant)
        {
            using (var conn = Db.GetConnection())
            {
                return conn.Query<UserRight>(
                    "dbo.PCN_Dashboard_Rights",
                    new { UserID = userId, Plant = plant },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}