using System.Collections.Generic;
using System.Data;

namespace TwitchBotApi.Extensions
{
    public static class ListStringExtensions
    {
        public static DataTable ToDataTable(this List<string> list)
        {
            DataTable table = new DataTable();
            
            table.Columns.Add();

            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
        }
    }
}
