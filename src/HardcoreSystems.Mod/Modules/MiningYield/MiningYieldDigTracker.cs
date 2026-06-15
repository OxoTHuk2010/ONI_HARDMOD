using System.Collections.Generic;

namespace HardcoreSystems.Modules.MiningYield
{
    public sealed class MiningYieldDigTracker
    {
        private readonly object syncRoot = new object();
        private readonly HashSet<int> pendingCells = new HashSet<int>();

        public void RecordDigCell(int gameCell)
        {
            lock (syncRoot)
            {
                pendingCells.Add(gameCell);
            }
        }

        public bool TryConsumeDigCell(int gameCell)
        {
            lock (syncRoot)
            {
                if (!pendingCells.Contains(gameCell))
                {
                    return false;
                }

                pendingCells.Remove(gameCell);
                return true;
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                pendingCells.Clear();
            }
        }
    }
}
