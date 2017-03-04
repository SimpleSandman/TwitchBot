using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Extensions
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
