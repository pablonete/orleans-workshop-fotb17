using Interface;
using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser
    {
        public async Task<bool> AddFriend(IUser friend)
        {
            var ok = await friend.InviteFriend(this);
            if (!ok)
            {
                return false;
            }

            if (!this.State.Friends.Contains(friend))
            {
                this.State.Friends.Add(friend);
            }

            await this.WriteStateAsync();

            return true;
        }

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(this.State);
        }

        public async Task<bool> InviteFriend(IUser friend)
        {
            if (!this.State.Friends.Contains(friend))
            {
                this.State.Friends.Add(friend);
            }

            await this.WriteStateAsync();
            return true;
        }

        public Task SetName(string name)
        {
            this.State.Name = name;
            return this.WriteStateAsync();
        }

        public Task SetStatus(string status)
        {
            this.State.Status = status;
            return this.WriteStateAsync();
        }
    }
}
