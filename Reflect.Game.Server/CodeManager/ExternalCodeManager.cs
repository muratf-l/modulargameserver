using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Reflect.GameServer.Library.Logging;

namespace Reflect.GameServer.CodeManager
{
    public class ExternalCodeManager
    {
        public enum ThreadState
        {
            AboutToTransitionToExternalCode,
            RunningExternalCode,
            RunningOurCode,
            Aborted,
            OutOfScope
        }

        private const int MaxStatusCheckThreadsAbortingOtherThreads = 5;
        public static readonly ExternalCodeManager Default = new ExternalCodeManager();
        private readonly ConcurrentDictionary<int, RunInfo> _runInfos = new ConcurrentDictionary<int, RunInfo>();
        private readonly Timer _statusCheckTimer;
        private readonly Stopwatch _timeSinceLastSkippedStatusCheckMessage = Stopwatch.StartNew();
        public int abortCount;
        private long _defaultMaxNaturalTimeTicks;

        private long _defaultMaxUserModeTicks;

        // Fields
        public long MaxTicks;
        private int _skippedStatusChecks;
        private int _statusCheckThreadsAbortingOtherThreads;

        // Methods
        private ExternalCodeManager()
        {
            _statusCheckTimer = new Timer(statusCheck);
        }

        // Properties
        public int MaxMilliseconds
        {
            get =>
                (int) TimeSpan.FromTicks(MaxTicks).TotalMilliseconds;
            set
            {
                if (Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    var ticks = TimeSpan.FromMilliseconds(value).Ticks;
                    if (ticks != MaxTicks)
                    {
                        MaxTicks = ticks;
                        _defaultMaxUserModeTicks = 3L * MaxTicks;
                        _defaultMaxNaturalTimeTicks = 10 * MaxTicks;
                        if (value > 0)
                        {
                            _statusCheckTimer.Change(value, value);
                        }
                        else
                        {
                            _statusCheckTimer.Change(-1, -1);
                            _runInfos.Clear();
                        }
                    }
                }
            }
        }

        public int AbortCount =>
            Thread.VolatileRead(ref abortCount);

        public void PauseToRunOurCode(Action ourCode)
        {
            PauseToRunOurCode<object>(delegate
            {
                ourCode();
                return null;
            });
        }

        public T PauseToRunOurCode<T>(Func<T> ourCode)
        {
            RunInfo info = null;
            ThreadState state;
            ThreadState state2;
            T local;
            if (MaxTicks <= 0L || !_runInfos.TryGetValue(Thread.CurrentThread.ManagedThreadId, out info))
                return ourCode();
            info.actOnThread();
            info.transitionToRunningOurCode(out state, out state2);
            if (state == ThreadState.RunningOurCode) return ourCode();
            info.checkForThreadAbortBeforeContinuing(state2);
            try
            {
                local = ourCode();
            }
            finally
            {
                info.transitionToRunningExternalCode(out state, out state2);
            }

            info.checkForThreadAbortBeforeContinuing(state2);
            return local;
        }

        public int ResetAbortCount()
        {
            return Interlocked.Exchange(ref abortCount, 0);
        }

        public void RunExternalCode(string gameId, Action externalCode)
        {
            long num;
            TimeSpan? maxNaturalTimeOverride = null;
            RunExternalCode<object>(gameId, delegate
            {
                externalCode();
                return null;
            }, maxNaturalTimeOverride, out num);
        }

        public T RunExternalCode<T>(string gameId, Func<T> externalCode)
        {
            long num;
            TimeSpan? maxNaturalTimeOverride = null;
            return RunExternalCode(gameId, externalCode, maxNaturalTimeOverride, out num);
        }

        public T RunExternalCode<T>(string gameId, Func<T> externalCode, TimeSpan? maxNaturalTimeOverride, out long ticks)
        {
            T local2;
            if (MaxTicks <= 0L)
            {
                ticks = 0L;
                return externalCode();
            }

            var topLevel = false;
            var initalThreadPriority = ThreadPriority.Normal;
            var currentThread = Thread.CurrentThread;

            var orAdd = _runInfos.GetOrAdd(currentThread.ManagedThreadId, delegate
            {
                topLevel = true;

                initalThreadPriority = currentThread.Priority;

                var info = new RunInfo(this)
                {
                    thread = currentThread
                };
                if (maxNaturalTimeOverride != null)
                {
                    info.maxNaturalTimeTicks = maxNaturalTimeOverride.Value.Ticks;
                    info.maxUserModeTicks = info.maxNaturalTimeTicks;
                }
                else
                {
                    info.maxNaturalTimeTicks = _defaultMaxNaturalTimeTicks;
                    info.maxUserModeTicks = _defaultMaxUserModeTicks;
                }

                info.gameId = gameId;
                return info;
            });

            var outOfScope = ThreadState.OutOfScope;

            var newThreadState = ThreadState.OutOfScope;

            try
            {
                T local;
                try
                {
                    orAdd.transitionToRunningExternalCode(out outOfScope, out newThreadState);
                    orAdd.checkForThreadAbortBeforeContinuing(newThreadState);
                    local = externalCode();
                }
                finally
                {
                    if (outOfScope == ThreadState.RunningOurCode)
                    {
                        ThreadState state3;
                        orAdd.transitionToRunningOurCode(out state3, out newThreadState);
                    }

                    ticks = orAdd.naturalTime.ElapsedTicks;
                }

                orAdd.checkForThreadAbortBeforeContinuing(newThreadState);
                local2 = local;
            }
            catch (Exception exception)
            {
                if (!topLevel || newThreadState != ThreadState.Aborted) throw;
                Thread.ResetAbort();
                var abortReason = orAdd.abortReason;
                Thread.MemoryBarrier();

                //if (log.IsInfoEnabled)
                //{
                //    object[] args = {abortReason, exception};
                //    log.Info("intercepted and reset abort on thread. abort reason was: {0} external call stack was {1}",
                //        args);
                //    object[] objArray2 = {Environment.StackTrace};
                //    log.Info("GS call stack is {0}", objArray2);
                //}

                throw new ExternalCodeManagerException(abortReason, exception);
            }
            finally
            {
                if (topLevel)
                {
                    Thread.VolatileWrite(ref orAdd.threadState, 4);

                    if (!_runInfos.TryRemove(currentThread.ManagedThreadId, out orAdd))
                        LogService.Error("error in logic. could not remove thread info");

                    orAdd.thread.Priority = initalThreadPriority;
                    orAdd.Dispose();
                }
            }

            return local2;
        }

        private void statusCheck(object dummy)
        {
            try
            {
                foreach (var pair in from ti in _runInfos
                    where !ti.Value.thread.IsAlive
                    select ti)
                {
                    RunInfo info;

                    object[] args = {pair.Key};
                    //log.Error("error in logic. holding onto non alive Thread {0}", args);

                    if (_runInfos.TryRemove(pair.Key, out info)) info.Dispose();
                }

                if (Interlocked.Increment(ref _statusCheckThreadsAbortingOtherThreads) > 5)
                    Interlocked.Increment(ref _skippedStatusChecks);
                else
                    foreach (var pair2 in _runInfos)
                        try
                        {
                            pair2.Value.isAbortFromStatusCheckExecuting = true;
                            pair2.Value.actOnThread();
                            pair2.Value.isAbortFromStatusCheckExecuting = false;
                        }
                        catch (Exception exception)
                        {
                            //log.ErrorException("external code manager statuscheck1", exception, new object[0]);
                        }

                Interlocked.Decrement(ref _statusCheckThreadsAbortingOtherThreads);
                if (_skippedStatusChecks > 0 && _timeSinceLastSkippedStatusCheckMessage.ElapsedMilliseconds > 0x2710L)
                {
                    var num = Interlocked.Exchange(ref _skippedStatusChecks, 0);
                    if (num > 0)
                    {
                        _timeSinceLastSkippedStatusCheckMessage.Restart();
                        var values = from a in _runInfos
                            where a.Value.isAbortFromStatusCheckExecuting &&
                                  Thread.VolatileRead(ref a.Value.threadState) == 3
                            select string.Format(
                                "GameId {0,5}, thread id {1,4}, cpuTime = {2}, totalTime = {3}, abortReason = {4}",
                                a.Value.gameId, a.Value.thread.ManagedThreadId,
                                TimeSpan.FromTicks(a.Value.cpuTime.ElapsedTicks), a.Value.totalTime.Elapsed,
                                a.Value.abortReason);
                        object[] args = {num, 5};
                        //log.Error(
                        //    "status check has skipped aborting threads {0} times because {1} other status check threads have been running at the same time (probably they are blocked running thread.Abort()",
                        //    args);
                        object[] objArray3 = {string.Join(Environment.NewLine, values)};
                        //log.Error("here are the threads that may be running Thread.Abort now\n{0}", objArray3);
                    }
                }
            }
            catch (Exception exception2)
            {
                //log.ErrorException("external code manager statuscheck2", exception2, new object[0]);
            }
        }
    }
}