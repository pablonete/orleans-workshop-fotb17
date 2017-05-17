using Interface;
using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser
    {
        private Random rand;
        public override Task OnActivateAsync()
        {
            this.rand = new Random(this.GetHashCode());

            this.RegisterTimer(this.OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            return base.OnActivateAsync();
        }

        private async Task OnTimer(object state)
        {
            if (this.State.Friends.Count > 0)
            {
                var luckyFriend = this.State.Friends.ToList()[this.rand.Next(this.State.Friends.Count)];
                await luckyFriend.Poke(this, "I'm bored!");
            }
        }

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

        public Task Poke(IUser user, string message)
        {
            Console.WriteLine($"User {user.GetPrimaryKeyString()} poked ${this.GetPrimaryKeyString()} with '{message}'");
            return Task.CompletedTask;
        }
    }
}
