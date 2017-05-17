using Interface;
using Orleans;
using System.Threading.Tasks;

namespace Grains
{
    public class UserGrain : Grain, IUser
    {
        private readonly UserProperties props = new UserProperties();

        public async Task<bool> AddFriend(IUser friend)
        {
            var ok = await friend.InviteFriend(this);
            if (!ok)
            {
                return false;
            }

            if (!this.props.Friends.Contains(friend))
            {
                this.props.Friends.Add(friend);
            }

            return true;
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(this.props);
        }

        public Task<bool> InviteFriend(IUser friend)
        {
            if (!this.props.Friends.Contains(friend))
            {
                this.props.Friends.Add(friend);
            }

            return Task.FromResult(true);
        }

        public Task SetName(string name)
        {
            this.props.Name = name;
            return Task.CompletedTask;
        }

        public Task SetStatus(string status)
        {
            this.props.Status = status;
            return Task.CompletedTask;
        }
    }
}
