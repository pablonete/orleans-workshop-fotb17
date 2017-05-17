using Interface;
using Orleans;
using System.Threading.Tasks;

namespace Grains
{
    public class UserGrain : Grain, IUser
    {
        private readonly UserProperties props = new UserProperties();

        public Task<UserProperties> GetProperties()
        {
            return Task.FromResult(this.props);
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
