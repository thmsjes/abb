using Abb.DTOs;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ABB_API_plateform.Data
{

    public interface IImagesClass
    {
        Task<int> InsertAsync(ImageDTO img);
        Task<List<ImageDTO>> GetByPropertyIdAsync(int propertyId);
        Task<List<ImageDTO>> GetFilteredAsync(int propertyId, string? year, string? category);
        Task<ImageDTO?> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }

    public class ImagesClass : IImagesClass 
    {
        private readonly string _connectionString;

        public ImagesClass(IConfiguration config)
        {
            try
            {


                if (config == null)
                {
                    throw new ArgumentNullException(nameof(config), "IConfiguration is null");
                }

                _connectionString = config.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection string is null or empty");
                }

       
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<int> InsertAsync(ImageDTO img)
        {
            const string sql = @"
                INSERT INTO [Images] ([PropertyId], [Year], [Category], [ImageName], [FilePath], [CreatedDate], [EventId])
                VALUES (@PropertyId, @Year, @Category, @ImageName, @FilePath, @CreatedDate, @EventId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.ExecuteScalarAsync<int>(sql, img);
        }

        public async Task<List<ImageDTO>> GetByPropertyIdAsync(int propertyId)
        {
            const string sql = @"
                SELECT [Id], [PropertyId], [Year], [Category], [ImageName], [FilePath], [CreatedDate], [EventId]
                FROM [Images] 
                WHERE [PropertyId] = @PropertyId 
                ORDER BY [CreatedDate] DESC";

            using IDbConnection db = new SqlConnection(_connectionString);
            var result = await db.QueryAsync<ImageDTO>(sql, new { PropertyId = propertyId });
            return result.ToList();
        }

        public async Task<List<ImageDTO>> GetFilteredAsync(int propertyId, string? year, string? category)
        {
            var sql = @"
                SELECT [Id], [PropertyId], [Year], [Category], [ImageName], [FilePath], [CreatedDate], [EventId]
                FROM [Images] 
                WHERE [PropertyId] = @PropertyId";

            var parameters = new DynamicParameters();
            parameters.Add("PropertyId", propertyId);

            if (!string.IsNullOrEmpty(year))
            {
                sql += " AND [Year] = @Year";
                parameters.Add("Year", year);
            }

            if (!string.IsNullOrEmpty(category))
            {
                sql += " AND [Category] = @Category";
                parameters.Add("Category", category);
            }

            sql += " ORDER BY [CreatedDate] DESC";

            using IDbConnection db = new SqlConnection(_connectionString);
            var result = await db.QueryAsync<ImageDTO>(sql, parameters);
            return result.ToList();
        }

        public async Task<ImageDTO?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT [Id], [PropertyId], [Year], [Category], [ImageName], [FilePath], [CreatedDate], [EventId]
                FROM [Images] 
                WHERE [Id] = @Id";

            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.QuerySingleOrDefaultAsync<ImageDTO>(sql, new { Id = id });
        }

        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM [Images] WHERE [Id] = @Id";

            using IDbConnection db = new SqlConnection(_connectionString);
            await db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
