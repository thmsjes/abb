using Dapper;
using System.Data;

namespace ABB_API_plateform.Infrastructure
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            return DateOnly.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue); // Convert DateOnly to DateTime
        }
    }

    public class DateOnlyNullableTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override DateOnly? Parse(object? value)
        {
            if (value == null || value is DBNull)
                return null;
                
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            return DateOnly.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.HasValue 
                ? value.Value.ToDateTime(TimeOnly.MinValue) 
                : (object)DBNull.Value;
        }
    }
}