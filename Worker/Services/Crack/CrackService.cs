using System.Security.Cryptography;
using System.Text;
using Worker.Services.Crack.Models;

namespace Worker.Services.Crack
{
    public interface ICrackService
    {
        public Task<List<string>> CrackAsync(CrackRequest request);
    }

    public class CrackService : ICrackService
    {
        private readonly IWordHandler _wordHandler;
        private readonly ILogger<CrackService> _logger;

        public CrackService(IWordHandler wordHandler, ILogger<CrackService> logger) {
            _wordHandler = wordHandler;
            _logger = logger;
        }

        public async Task<List<string>> CrackAsync(CrackRequest request)
        {
            return await Task.Run(() =>
            {
                return Crack(request);
            });
        }

        private List<string> Crack(CrackRequest request)
        {
            var result = new List<string>();

            _wordHandler.InitializeHandler(request);

            foreach(var word in _wordHandler.GetLeftWords()) {
                var hash = GetMd5Hash(word);
                if (hash == request.Hash) {
                    result.Add(word);
                }
            }

            _logger.LogInformation("part " + request.PartNumber + " checked all the words");
            return result;
        }

        private string GetMd5Hash(string input) {
            using MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes) {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
