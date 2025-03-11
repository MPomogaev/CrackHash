using Common;

namespace Manager.Services.Models
{
    public class WorkerTask
    {
        public Guid RequestId { get; set; }
        public RequestState State { get; set; } = RequestState.InProgress;
        public DateTime StartTime { get; set; }
        public int ExpectedPartsCount { get; set; }
        public HashSet<CrackHashWorkerResponse> ReceivedParts { get; set; } = new();

    }

    public enum RequestState
    {
        InProgress,
        Error,
        Ready
    }

    public static class RequestStates
    {

        public static string ToString(this RequestState state)
        {
            return RequestStatesDict[state];
        }

        private static readonly Dictionary<RequestState, string> RequestStatesDict = new() {
            { RequestState.InProgress, "IN_PROGRESS" },
            { RequestState.Ready, "READY" },
            { RequestState.Error, "ERROR" }
        };
    }

}
