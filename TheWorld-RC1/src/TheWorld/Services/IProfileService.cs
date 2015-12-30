using System;
using System.Threading.Tasks;

namespace TheWorld.Services
{
    public interface IProfileService
    {
         Task<byte[]> GetUserProfileImage(string userEmail);
         Task<string> GetBase64UserProfileImage(string userEmail);
    }
}
