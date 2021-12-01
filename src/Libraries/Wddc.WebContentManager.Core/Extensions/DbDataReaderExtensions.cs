using System.Data.Common;

namespace Wddc.WebContentManager.Core.Extensions
{
    public static class DbDataReaderExtensions
    {
        public static string SafeGetString(this DbDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }
    }
}
