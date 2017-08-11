// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using Lifti.Locking;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the <see cref="LockManager"/> class.
    /// </summary>
    [TestFixture]
    public class LockManagerTests
    {
        /// <summary>
        /// Tests that obtaining a read lock causes the read lock acquired event to be raised. 
        /// The release lock counterpart should automatically occur when the lock is disposed.
        /// </summary>
        [Test]
        public void ObtainingReadLockCausesEventToBeRaised()
        {
            var output = new List<string>();
            var lockManager = new LockManager();
            lockManager.ReadLockAcquired += (s, e) => output.Add("RL Acquired");
            lockManager.ReadLockReleased += (s, e) => output.Add("RL Released");
            lockManager.WriteLockAcquired += (s, e) => output.Add("WL Acquired");
            lockManager.WriteLockReleased += (s, e) => output.Add("WL Released");

            using (lockManager.AcquireReadLock())
            {
            }

            Assert.IsTrue(output.SequenceEqual(new[] { "RL Acquired", "RL Released" }));
        }

        /// <summary>
        /// Tests that obtaining a write lock causes the read lock acquired event to be raised. 
        /// The release lock counterpart should automatically occur when the lock is disposed.
        /// </summary>
        [Test]
        public void ObtainingWriteLockCausesEventToBeRaised()
        {
            var output = new List<string>();
            var lockManager = new LockManager();
            lockManager.ReadLockAcquired += (s, e) => output.Add("RL Acquired");
            lockManager.ReadLockReleased += (s, e) => output.Add("RL Released");
            lockManager.WriteLockAcquired += (s, e) => output.Add("WL Acquired");
            lockManager.WriteLockReleased += (s, e) => output.Add("WL Released");

            using (lockManager.AcquireWriteLock())
            {
            }

            Assert.IsTrue(output.SequenceEqual(new[] { "WL Acquired", "WL Released" }));
        }

        /// <summary>
        /// Tests that obtaining a read lock causes no read lock acquired events to be raised
        /// when the lock manager is disabled.
        /// </summary>
        [Test]
        public void ObtainingReadLockCausesNoEventsToBeRaisedWhenDisabled()
        {
            var output = new List<string>();
            var lockManager = new LockManager { Enabled = false };
            lockManager.ReadLockAcquired += (s, e) => output.Add("RL Acquired");
            lockManager.ReadLockReleased += (s, e) => output.Add("RL Released");
            lockManager.WriteLockAcquired += (s, e) => output.Add("WL Acquired");
            lockManager.WriteLockReleased += (s, e) => output.Add("WL Released");

            using (lockManager.AcquireReadLock())
            {
            }

            Assert.IsTrue(output.Count() == 0);
        }

        /// <summary>
        /// Tests that obtaining a write lock causes no write lock acquired events to be raised
        /// when the lock manager is disabled
        /// </summary>
        [Test]
        public void ObtainingWriteLockCausesNoEventsToBeRaisedWhenDisabled()
        {
            var output = new List<string>();
            var lockManager = new LockManager { Enabled = false };
            lockManager.ReadLockAcquired += (s, e) => output.Add("RL Acquired");
            lockManager.ReadLockReleased += (s, e) => output.Add("RL Released");
            lockManager.WriteLockAcquired += (s, e) => output.Add("WL Acquired");
            lockManager.WriteLockReleased += (s, e) => output.Add("WL Released");

            using (lockManager.AcquireWriteLock())
            {
            }

            Assert.IsTrue(output.Count() == 0);
        }

        /// <summary>
        /// Tests that multiple read locks are allowed simultaneously.
        /// </summary>
        [Test]
        public void MultipleReadLocksShouldBeAllowedSimultaneously()
        {
            var threadActions = new[]
            {
                new ThreadActionData(0, ThreadAction.AcquireReadLock),
                new ThreadActionData(1, ThreadAction.AcquireReadLock),
                new ThreadActionData(2, ThreadAction.AcquireReadLock),
                new ThreadActionData(0, ThreadAction.ReleaseLock),
                new ThreadActionData(2, ThreadAction.ReleaseLock),
                new ThreadActionData(1, ThreadAction.ReleaseLock)
            };

            var tester = new ThreadTestContainer(threadActions, 3);
            tester.ExecuteTests();

            var expectedResults = new[]
            {
                "0: RL Acquired",
                "1: RL Acquired",
                "2: RL Acquired",
                "0: RL Released",
                "2: RL Released",
                "1: RL Released"
            };

            Assert.IsTrue(tester.Results.SequenceEqual(expectedResults));
        }

        /// <summary>
        /// Tests that read locks are blocked while a write lock is active.
        /// </summary>
        [Test]
        public void ReadLocksShouldBlockedWhileWriteLockIsActive()
        {
            var threadActions = new[]
            {
                new ThreadActionData(0, ThreadAction.AcquireWriteLock),
                new ThreadActionData(1, ThreadAction.AcquireReadLock, expectActionToBlock: true),
                new ThreadActionData(2, ThreadAction.AcquireReadLock, expectActionToBlock: true),
                new ThreadActionData(0, ThreadAction.ReleaseLock, unblockedThreadCount: 2),
                new ThreadActionData(1, ThreadAction.ReleaseLock),
                new ThreadActionData(2, ThreadAction.ReleaseLock)
            };

            var tester = new ThreadTestContainer(threadActions, 3);
            tester.ExecuteTests();

            // Can only verify the first two entries - the order in which the
            // read locks are granted is non-deterministic - it depends on the order
            // the threads get services. For this test that doesn't matter - the important
            // thing is that the write lock is released before any reads are granted
            Assert.AreEqual("0: WL Acquired", tester.Results[0]);
            Assert.AreEqual("0: WL Released", tester.Results[1]);
        }

        /// <summary>
        /// Tests that write locks are blocked while a read lock is active.
        /// </summary>
        [Test]
        public void WriteLocksShouldBlockedWhileReadLockIsActive()
        {
            var threadActions = new[]
            {
                new ThreadActionData(0, ThreadAction.AcquireReadLock),
                new ThreadActionData(1, ThreadAction.AcquireWriteLock, expectActionToBlock: true),
                new ThreadActionData(0, ThreadAction.ReleaseLock, unblockedThreadCount: 1),
                new ThreadActionData(1, ThreadAction.ReleaseLock)
            };

            var tester = new ThreadTestContainer(threadActions, 2);
            tester.ExecuteTests();

            var expectedResults = new[]
            {
                "0: RL Acquired",
                "0: RL Released",
                "1: WL Acquired",
                "1: WL Released"
            };

            Assert.IsTrue(tester.Results.SequenceEqual(expectedResults));
        }

        /// <summary>
        /// Tests that write locks are blocked while another write lock is active.
        /// </summary>
        [Test]
        public void WriteLocksShouldBlockWhileWriteLockIsPending()
        {
            var threadActions = new[]
            {
                new ThreadActionData(0, ThreadAction.AcquireWriteLock),
                new ThreadActionData(1, ThreadAction.AcquireWriteLock, expectActionToBlock: true),
                new ThreadActionData(0, ThreadAction.ReleaseLock, unblockedThreadCount: 1),
                new ThreadActionData(1, ThreadAction.ReleaseLock)
            };

            var tester = new ThreadTestContainer(threadActions, 2);
            tester.ExecuteTests();

            var expectedResults = new[]
            {
                "0: WL Acquired",
                "0: WL Released",
                "1: WL Acquired",
                "1: WL Released"
            };

            Assert.IsTrue(tester.Results.SequenceEqual(expectedResults));
        }

        /// <summary>
        /// The various actions a thread can take in a unit test.
        /// </summary>
        private enum ThreadAction
        {
            /// <summary>
            /// A read lock should be acquired.
            /// </summary>
            AcquireReadLock,

            /// <summary>
            /// A write lock should be acquired.
            /// </summary>
            AcquireWriteLock,

            /// <summary>
            /// The currently held lock should be released.
            /// </summary>
            ReleaseLock
        }

        /// <summary>
        /// Information about an action to be taken by a thread.
        /// </summary>
        private struct ThreadActionData
        {
            /// <summary>
            /// The actor id.
            /// </summary>
            private readonly int actorId;

            /// <summary>
            /// The action to perform.
            /// </summary>
            private readonly ThreadAction action;

            /// <summary>
            /// The number of blocked threads that are expected to be released 
            /// by this action.
            /// </summary>
            private readonly int unblockedThreadCount;

            /// <summary>
            /// Whether this action is expected to initially be
            /// blocked. This affects whether or not a thread waits to join the other
            /// threads once its action completes
            /// </summary>
            private readonly bool expectActionToBlock;

            /// <summary>
            /// Initializes a new instance of the <see cref="LockManagerTests.ThreadActionData"/> struct.
            /// </summary>
            /// <param name="actorId">The id of the actor thread that should perform this action.</param>
            /// <param name="action">The action the thread should perform.</param>
            /// <param name="unblockedThreadCount">The number of threads that are expected to be unblocked by this action.
            /// This is used to ensure that the correct number of threads are synchronized when the action
            /// has completed.</param>
            /// <param name="expectActionToBlock">Whether this action is expected to initially be
            /// blocked. This affects whether or not a thread waits to join the other
            /// threads once its action completes.</param>
            public ThreadActionData(int actorId, ThreadAction action, int unblockedThreadCount = 0, bool expectActionToBlock = false)
            {
                this.actorId = actorId;
                this.action = action;
                this.unblockedThreadCount = unblockedThreadCount;
                this.expectActionToBlock = expectActionToBlock;
            }

            /// <summary>
            /// Gets the id of the actor thread that should perform this action.
            /// </summary>
            /// <value>The actor id.</value>
            public int ActorId
            {
                get
                {
                    return this.actorId;
                }
            }

            /// <summary>
            /// Gets the action the thread should perform.
            /// </summary>
            /// <value>The thread action.</value>
            public ThreadAction Action
            {
                get
                {
                    return this.action;
                }
            }

            /// <summary>
            /// Gets the number of threads that are expected to be unblocked by this action.
            /// This is used to ensure that the correct number of threads are synchronized when the action
            /// has completed.
            /// </summary>
            /// <value>The expected number of threads that will be unblocked.</value>
            public int ExpectedUnblockedThreadCount
            {
                get
                {
                    return this.unblockedThreadCount;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this action is expected to initially be
            /// blocked. This affects whether or not a thread waits to join the other
            /// threads once its action completes.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this action is expected to block; otherwise, <c>false</c>.
            /// </value>
            public bool ExpectActionToBlock
            {
                get
                {
                    return this.expectActionToBlock;
                }
            }
        }

        /// <summary>
        /// A helper class that encapsulates a multi-threaded lock test instance.
        /// </summary>
        private class ThreadTestContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LockManagerTests.ThreadTestContainer"/> class.
            /// </summary>
            /// <param name="actions">The thread actions to take in the order they should occur.</param>
            /// <param name="threadCount">The number of threads that will be used to perform the actions.</param>
            public ThreadTestContainer(IEnumerable<ThreadActionData> actions, int threadCount)
            {
                Assert.IsTrue(
                    actions.All(a => a.ActorId < threadCount),
                    "Invalid test setup - provided actor ids too large for thread count");

                this.threadCount = threadCount;
                this.lockManager = new LockManager();
                this.lockManager.ReadLockAcquired += (s, e) => this.Results.Add(threadActorId + ": RL Acquired");
                this.lockManager.ReadLockReleased += (s, e) => this.Results.Add(threadActorId + ": RL Released");
                this.lockManager.WriteLockAcquired += (s, e) => this.Results.Add(threadActorId + ": WL Acquired");
                this.lockManager.WriteLockReleased += (s, e) => this.Results.Add(threadActorId + ": WL Released");

                this.threadActions = new Queue<ThreadActionData>(actions);
                this.Results = new List<string>(this.threadActions.Count);
            }

            /// <summary>
            /// Gets the results of running the test.
            /// </summary>
            /// <value>The list of strings that represent the sequence of actions that occurred.</value>
            public List<string> Results
            {
                get; }

            /// <summary>
            /// Executes the configured actions on the configured number of threads.
            /// </summary>
            public void ExecuteTests()
            {
                var threads = (from i in Enumerable.Range(0, this.threadCount)
                               select new Thread(this.ThreadExecute)
                                   {
                                       Name = "Actor thread " + i, 
                                       IsBackground = true
                                   }).ToArray();

                this.actionStartBarrier = this.CreateActionStartBarrier();

                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i].Start(i);
                }

                try
                {
                    var stepNumber = 0;
                    while (this.threadActions.Count > 0)
                    {
                        var action = this.threadActions.Dequeue();
                        this.currentAction = action;
                        Console.WriteLine(
                            "^ Step:{0} Thread:{1} Action:{2} ^", stepNumber++, action.ActorId, action.Action);

                        // Set up the barrier to synchronize this and the executing 
                        // threads before moving on to the next action.
                        if (action.ExpectActionToBlock)
                        {
                            this.currentlyBlockingThreads += 1;
                        }

                        this.currentlyBlockingThreads -= action.ExpectedUnblockedThreadCount;

                        var expectedThreadsAtBarrier = this.threadCount - this.currentlyBlockingThreads;
                        Console.WriteLine("Expecting {0} threads to meet at barrier", expectedThreadsAtBarrier);
                        this.actionCompletedBarrier = new Barrier(expectedThreadsAtBarrier + 1);

                        // Join the starting barrier - this will be cleared when all the threads are ready to go
                        if (!this.actionStartBarrier.SignalAndWait(Debugger.IsAttached ? 120000 : 2000))
                        {
                            Assert.Fail("Deadlock encountered waiting for start barrier - check test setup");
                        }

                        // Wait on the barrier - in case there has been a test configuration error
                        // a timeout is specified - this will cause the test to fail in case of a
                        // deadlock caused by waiting on a blocked thread
                        if (!this.actionCompletedBarrier.SignalAndWait(Debugger.IsAttached ? 120000 : 2000))
                        {
                            Assert.Fail("Deadlock encountered waiting for completion barrier - check test setup");
                        }

                        if (this.raisedException != null)
                        {
                            // Re-raise the exception on the main test thread - that way it will get reported properly
                            throw new Exception("Re-raised thread exception", this.raisedException);
                        }

                        if (action.ExpectActionToBlock)
                        {
                            // Hold up the main thread to ensure that the thread that is expected to block
                            // has actually blocked
                            Thread.Sleep(10);
                        }
                    }
                }
                finally
                {
                    // End the test
                    this.completed = true;
                    Console.WriteLine("Main test thread completed");
                    this.actionStartBarrier.SignalAndWait(10);
                }
            }

            /// <summary>
            /// The number of threads being processed.
            /// </summary>
            private readonly int threadCount;

            /// <summary>
            /// The thread actions to execute.
            /// </summary>
            private readonly Queue<ThreadActionData> threadActions;

            /// <summary>
            /// The lock manager to perform the tests on.
            /// </summary>
            private readonly ILockManager lockManager;

            /// <summary>
            /// The current lock acquired by a thread.
            /// </summary>
            [ThreadStatic]
            private static ILock currentThreadLock;

            /// <summary>
            /// The actor id of the current thread.
            /// </summary>
            [ThreadStatic]
            private static int threadActorId;

            /// <summary>
            /// The barrier threads will meet at after an action has been executed.
            /// </summary>
            private Barrier actionCompletedBarrier;

            /// <summary>
            /// The barrier threads will meet at before an is to be executed.
            /// </summary>
            private Barrier actionStartBarrier;

            /// <summary>
            /// Set to <c>true</c> when all actions have been processed. Threads should exit when this occurs.
            /// </summary>
            private bool completed;

            /// <summary>
            /// The currently executing action.
            /// </summary>
            private ThreadActionData currentAction;

            /// <summary>
            /// Gets the current number of threads that are expected to be blocking.
            /// </summary>
            private int currentlyBlockingThreads;

            /// <summary>
            /// An exception that was raised on a thread. This will get re-raised on the test runner thread to prevent
            /// wierd warnings being logged by the test framework.
            /// </summary>
            private Exception raisedException;

            /// <summary>
            /// Creates the action start barrier.
            /// </summary>
            /// <returns>The barrier capable of rebuilding itself once it completes.</returns>
            private Barrier CreateActionStartBarrier()
            {
                // Create a new barrier that waits for the current number of active threads
                // plus one (for the main test thread). When the barrier completes, it automatically resets
                // the action start barrier again.
                return new Barrier(
                    (this.threadCount - this.currentlyBlockingThreads) + 1,
                    b => this.actionStartBarrier = this.CreateActionStartBarrier());
            }

            /// <summary>
            /// The method that runs for a test thread. When triggered executes the
            /// current action if it is targeting the current thread.
            /// </summary>
            /// <param name="data">The thread data containing an int representing the thread's actor id.</param>
            private void ThreadExecute(object data)
            {
                try
                {
                    threadActorId = (int)data;

                    while (true)
                    {
                        // Wait for another action to be made available
                        Console.WriteLine("# Actor {0} waiting at start barrier #", threadActorId);
                        this.actionStartBarrier.SignalAndWait();
                        Console.WriteLine("> Actor {0} running >", threadActorId);

                        // First check to see if the thread should end
                        if (this.completed)
                        {
                            break;
                        }

                        var action = this.currentAction;

                        // Make sure this thread should be processing the action
                        if (action.ActorId == threadActorId)
                        {
                            Console.WriteLine("Consuming action: Actor {0} Action {1}", action.ActorId, action.Action);
                            this.ExecuteThreadAction(action);
                        }

                        // Meet all the other threads at the barrier
                        Console.WriteLine("< Actor {0} meeting at barrier <", threadActorId);
                        this.actionCompletedBarrier.SignalAndWait();
                    }

                    Console.WriteLine("Actor {0} completed", threadActorId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("!!! Thread exception " + ex.Message);

                    // Store the exception
                    this.raisedException = ex;

                    // Make sure that the barrier is cleared for this instance
                    this.actionCompletedBarrier.SignalAndWait();
                }
            }

            /// <summary>
            /// Executes the given thread action.
            /// </summary>
            /// <param name="action">The action to execute.</param>
            private void ExecuteThreadAction(ThreadActionData action)
            {
                switch (action.Action)
                {
                    case ThreadAction.AcquireReadLock:
                        if (currentThreadLock != null)
                        {
                            throw new Exception("Config error - lock already obtained");
                        }

                        currentThreadLock = this.lockManager.AcquireReadLock();
                        Console.WriteLine("Actor {0} read lock obtained", threadActorId);
                        break;

                    case ThreadAction.AcquireWriteLock:
                        if (currentThreadLock != null)
                        {
                            throw new Exception("Config error - lock already obtained");
                        }

                        currentThreadLock = this.lockManager.AcquireWriteLock();
                        Console.WriteLine("Actor {0} write lock obtained", threadActorId);
                        break;

                    case ThreadAction.ReleaseLock:
                        if (currentThreadLock == null)
                        {
                            throw new Exception("Config error - no lock held");
                        }

                        currentThreadLock.Dispose();
                        currentThreadLock = null;
                        Console.WriteLine("Actor {0} lock released", threadActorId);
                        break;
                }
            }
        }
    }
}
