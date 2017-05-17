using Orleans;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interface
{
    public class UserProperties
    {
        public UserProperties()
        {
            this.Friends = new HashSet<IUser>();
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public HashSet<IUser> Friends { get; set; }

        public override string ToString()
        {
            string friends = string.Join(", ", this.Friends.Select(f => f.GetPrimaryKeyString()));
            return $"Name='{Name}' Status='{Status}' Friends='{friends}'";
        }
    }

    public interface IUser : IGrainWithStringKey
    {
        Task SetName(string name);
        Task SetStatus(string status);

        Task<bool> InviteFriend(IUser friend);
        Task<bool> AddFriend(IUser friend);

        Task<UserProperties> GetProperties();
    }
}
