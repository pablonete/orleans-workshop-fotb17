using Orleans;
using System.Threading.Tasks;

namespace Interface
{
    public class UserProperties
    {
        public string Name { get; set; }
        public string Status { get; set; }

        public override string ToString()
        {
            return $"Name='{Name}' Status='{Status}'";
        }
    }

    public interface IUser : IGrainWithStringKey
    {
        Task SetName(string name);
        Task SetStatus(string status);

        Task<UserProperties> GetProperties();
    }
}
