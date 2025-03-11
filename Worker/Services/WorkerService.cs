using Common;
using Worker.Services.Crack;
using Worker.Services.Crack.Models;

namespace Worker.Services
{
    public interface IWorkerService {
        public void CrackAsync(CrackHashManagerRequest request);
    }

    public class WorkerService: IWorkerService
    {
        private readonly ICrackService _crackService;
        private readonly IManagerApiService _managerApiService;
        

        public WorkerService(ICrackService crackService, 
            IManagerApiService managerApiService) {
            _crackService = crackService;
            _managerApiService = managerApiService;
        }

        public async void CrackAsync(CrackHashManagerRequest request) {
            var answers = await _crackService.CrackAsync(new CrackRequest {
                Alphabet = request.Alphabet,
                PartCount = request.PartCount,
                PartNumber = request.PartNumber,
                MaxLength = request.MaxLength,
                Hash = request.Hash
            });

            _managerApiService.SendAnswerAsync(new CrackHashWorkerResponse {
                RequestId = request.RequestId,
                PartNumber = request.PartNumber,
                Answers = answers
            });
        }

    }
}
