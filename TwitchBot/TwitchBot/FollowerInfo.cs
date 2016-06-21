using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class User
    {
        public int _id { get; set; }
        public string name { get; set; }
        //public string created_at { get; set; }
        //public string updated_at { get; set; }
        //public string display_name { get; set; }
        //public string logo { get; set; }
        //public string bio { get; set; }
        //public string type { get; set; }
    }

    public class Follow
    {
        //public string created_at { get; set; }
        //public bool notifications { get; set; }
        public User user { get; set; }
    }

    public class FollowerInfo
    {
        public List<Follow> follows { get; set; }
        public int _total { get; set; }
        //public string _cursor { get; set; }
    }
}
