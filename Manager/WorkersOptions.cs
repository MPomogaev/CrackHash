namespace Manager {
    public class WorkersOptions {
        public int WorkerCount { get; set; } = 1;

        public List<string> Alphabet { get; set; } 
            = "abcdefghijklmnopqrstuvwxyz0123456789".Select(ch => ch.ToString()).ToList();

        public int TimeoutInSeconds { get; set; } = 10;

    }
}
