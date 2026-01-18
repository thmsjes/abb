using Dapper;
using System.Data;

namespace Abb.Business
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        // How to save DateOnly to the Database
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
            parameter.DbType = DbType.Date;
        }

        // How to read DateOnly from the Database
        public override DateOnly Parse(object value)
        {
            return DateOnly.FromDateTime((DateTime)value);
        }
    }
}
