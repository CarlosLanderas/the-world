using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TheWorld.Services
{
    public class DefaultProfileService : IProfileService
    {
        private const string ImageSize = "200";
        
        public async Task<byte[]> GetUserProfileImage(string userEmail)
        {
            var hashImage = GetHashedEmail(userEmail).ToLower();
            var url = $"http://www.gravatar.com/avatar/{hashImage}.jpg?s={ImageSize}";

            HttpClient httpClient = new HttpClient();
            var imageStream=await httpClient.GetStreamAsync(url);

            using (var memoryStream = new MemoryStream())
            {
                imageStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }

        }

        public async Task<string> GetBase64UserProfileImage(string userEmail)
        {
            var imageByte = await GetUserProfileImage(userEmail);
            return Convert.ToBase64String(imageByte);
        }

        private string GetHashedEmail(string userEmail)
        {
            return CalculateMD5Hash(userEmail);
        }

        private string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
