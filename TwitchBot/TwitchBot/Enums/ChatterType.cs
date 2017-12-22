using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Enums
{
    public enum ChatterType
    {
        DoesNotExist,
        Viewer,
        Follower,
        RegularFollower,
        Moderator,
        GlobalModerator,
        Admin,
        Staff,
        Subscriber
    }
}
