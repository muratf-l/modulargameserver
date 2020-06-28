using System;
using System.Diagnostics;
using System.Threading;

namespace Reflect.GameServer.CodeManager
{
    public class RunInfo : IDisposable
    {
        private readonly ExternalCodeManager parent;
        public string abortReason;

        public string gameId;
        public volatile bool isAbortFromStatusCheckExecuting;
        public long maxNaturalTimeTicks;
        public long maxUserModeTicks;
        public Stopwatch naturalTime;
        public Thread thread;

        public int threadState;

        // Fields
        public Stopwatch totalTime;
        public Stopwatch cpuTime;

        // Methods
        public RunInfo(ExternalCodeManager parent)
        {
            this.parent = parent;
            threadState = 2;
            naturalTime = new Stopwatch();
            cpuTime = Stopwatch.StartNew();
            totalTime = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            
        }

        public void actOnThread()
        {
            string str;
            string str2;
            string str3;
            getReasonToActOnThreadNow(out str, out str2, out str3);
            if (str2 != null && Interlocked.CompareExchange(ref threadState, 3, 1) == 1)
            {
                abortReason = str2;
                //ExternalCodeManager.log.Warn(str3, new object[0]);
                Interlocked.Increment(ref parent.abortCount);
                thread.Abort();
                object[] args = {thread.ManagedThreadId};
                //ExternalCodeManager.log.Info("thread {0} aborted", args);
            }

            if (str != null && Thread.VolatileRead(ref threadState) != 4 &&
                thread.Priority != ThreadPriority.BelowNormal)
                thread.Priority = ThreadPriority.BelowNormal;
            //ExternalCodeManager.log.Warn(str, new object[0]);
        }

        public void checkForThreadAbortBeforeContinuing(ExternalCodeManager.ThreadState currentThreadState)
        {
            if (!ReferenceEquals(Thread.CurrentThread, thread))
            {
                object[] args = {thread.ManagedThreadId, Thread.CurrentThread.ManagedThreadId};
                //ExternalCodeManager.log.Error("error in logic. monitored thread id {0} is not the same as current thread id {1}", args);
            }
            else if (currentThreadState == ExternalCodeManager.ThreadState.Aborted)
            {
                thread.Abort();
            }
        }

        public void getReasonToActOnThreadNow(out string slowThreadPriorityReason, out string abortThreadReason,
            out string verboseAbortThreadReason)
        {
            slowThreadPriorityReason = null;
            abortThreadReason = null;
            verboseAbortThreadReason = null;
            var elapsed = naturalTime.Elapsed;
            if (elapsed.Ticks > parent.MaxTicks)
            {
                slowThreadPriorityReason =
                    $"Lowering priority of thread {thread.ManagedThreadId} to BelowNormal because it ran external code for more than {(int) elapsed.TotalMilliseconds} ms.";
                
                var elapsedTicks = cpuTime.ElapsedTicks;

                if (elapsedTicks > maxUserModeTicks || elapsed.Ticks > maxNaturalTimeTicks)
                {
                    var totalMilliseconds = (int) elapsed.TotalMilliseconds;
                    abortThreadReason =
                        $"Aborting thread because it ran for {totalMilliseconds} ms, which is more than allowed in your server's cluster.";
                    verboseAbortThreadReason =
                        $"Aborting thread {thread.ManagedThreadId} because it ran external code for {totalMilliseconds} ms, used the cpu for {(int) TimeSpan.FromTicks(elapsedTicks).TotalMilliseconds} ms and took {totalTime.ElapsedMilliseconds} ms in total.";
                }
            }
        }

        public void transitionToRunningExternalCode(out ExternalCodeManager.ThreadState previousThreadState,
            out ExternalCodeManager.ThreadState newThreadState)
        {
            var runningOurCode = ExternalCodeManager.ThreadState.RunningOurCode;
            newThreadState = ExternalCodeManager.ThreadState.AboutToTransitionToExternalCode;
            previousThreadState =
                (ExternalCodeManager.ThreadState) Interlocked.CompareExchange(ref threadState, (int) newThreadState,
                    (int) runningOurCode);
            if (previousThreadState != runningOurCode)
            {
                if (previousThreadState != newThreadState)
                {
                    if (previousThreadState != ExternalCodeManager.ThreadState.RunningExternalCode)
                    {
                        object[] args = {newThreadState, previousThreadState, runningOurCode, Environment.StackTrace};
                        //ExternalCodeManager.log.Warn(
                        //    "could not change thread state to {0} because it was {1} and not {2} as expected. call stack {3}",
                        //    args);
                    }

                    newThreadState = previousThreadState;
                }
            }
            else
            {
                naturalTime.Start();
                cpuTime.Start();
                runningOurCode = ExternalCodeManager.ThreadState.AboutToTransitionToExternalCode;
                newThreadState = ExternalCodeManager.ThreadState.RunningExternalCode;
                previousThreadState =
                    (ExternalCodeManager.ThreadState) Interlocked.CompareExchange(ref threadState, (int) newThreadState,
                        (int) runningOurCode);
                if (previousThreadState == runningOurCode)
                {
                    previousThreadState = ExternalCodeManager.ThreadState.RunningOurCode;
                }
                else
                {
                    object[] args = {newThreadState, previousThreadState, runningOurCode};
                    //ExternalCodeManager.log.Warn(
                    //    "could not change thread state to {0} because it was {1} and not {2} as expected", args);
                    newThreadState = previousThreadState;
                }
            }
        }

        public void transitionToRunningOurCode(out ExternalCodeManager.ThreadState previousThreadState,
            out ExternalCodeManager.ThreadState newThreadState)
        {
            var runningExternalCode = ExternalCodeManager.ThreadState.RunningExternalCode;
            newThreadState = ExternalCodeManager.ThreadState.RunningOurCode;
            previousThreadState =
                (ExternalCodeManager.ThreadState) Interlocked.CompareExchange(ref threadState, (int) newThreadState,
                    (int) runningExternalCode);
            if (previousThreadState == runningExternalCode)
            {
                naturalTime.Stop();
                cpuTime.Stop();
            }
            else if (previousThreadState != newThreadState)
            {
                if (previousThreadState != ExternalCodeManager.ThreadState.Aborted)
                {
                    object[] args = {newThreadState, previousThreadState, runningExternalCode};
                    //ExternalCodeManager.log.Warn(
                    //    "could not change thread state to {0} because it was {1} and not {2} as expected", args);
                }

                newThreadState = previousThreadState;
            }
        }
    }
}