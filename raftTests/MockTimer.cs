using raftLibrary;

namespace raftTests
{
    public class MockTimer : raftLibrary.ITimer
    {
        public int TimeoutMs { get; set; } = 1000;
        public int CurrentTimeoutMs {get; set;} = 1000;
        public int step { get; set; } = 5;
        public bool isReset = false;

        public void ResetTimeout()
        {
            isReset = true;
            CurrentTimeoutMs = TimeoutMs;
        }
    }
}