using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace Autofac.Annotation.Util
{
    /// <summary>
    /// 检测async await使用不规范，比如同步被异步的aop包裹，在异步aop里面用了await会导致死锁用的
    /// https://github.com/ramondeklein/deadlockdetection
    /// </summary>
    internal class DeadLockCheck : IDisposable
    {
        private readonly DeadlockDetectionSynchronizationContext _deadlockDetectionSynchronizationContext;

        /// <summary>
        /// Enable deadlock detection with the default deadlock detection mode.
        /// </summary>
        public static IDisposable Enable(string caller)
        {
            return DeadlockDetection(caller);
        }

        /// <summary>
        /// Enable deadlock detection with a specific deadlock detection mode.
        /// </summary>
        static IDisposable DeadlockDetection(string caller)
        {
            // Use deadlock detection
            return new DeadLockCheck(caller);
        }

        private DeadLockCheck(string caller)
        {
            // Determine the current synchronization context and abort if we
            // only want to find actual deadlocks and there is no synchronization context
            var currentSynchronizationContext = SynchronizationContext.Current;
            if (currentSynchronizationContext == null)
                return;

            // Install our deadlock detection synchronization context
            _deadlockDetectionSynchronizationContext = new DeadlockDetectionSynchronizationContext(currentSynchronizationContext,caller);
            SynchronizationContext.SetSynchronizationContext(_deadlockDetectionSynchronizationContext);
        }

        ~DeadLockCheck()
        {
            // We should always have been properly disposed, unless deadlock
            // detection has been disabled. No side effects should occur in
            // this mode.
            throw new InvalidOperationException("Always dispose the deadlock detection (tip: use the 'using' keyword).");
        }

        void IDisposable.Dispose()
        {
            // Don't do anything

            // Restore the original synchronization context if we installed our own
            if (_deadlockDetectionSynchronizationContext != null)
                SynchronizationContext.SetSynchronizationContext(_deadlockDetectionSynchronizationContext.BaseSynchronizationContext);

            // Don't run the finalizer
            GC.SuppressFinalize(this);
        }
    }

    [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
    internal class DeadlockDetectionSynchronizationContext : SynchronizationContext
    {
        private string _caller;
        private readonly object _sync = new object();
        private bool _isBlocking;
        private Thread _currentThread;

        public DeadlockDetectionSynchronizationContext(SynchronizationContext baseSynchronizationContext,string caller)
        {
            _caller = caller;
            // Save the underlying synchronization context
            BaseSynchronizationContext = baseSynchronizationContext;

            // We do want to have wait notifications
            SetWaitNotificationRequired();
        }

        public SynchronizationContext BaseSynchronizationContext { get; }

        public override void Post(SendOrPostCallback d, object state)
        {
            _currentThread = Thread.CurrentThread;

            //_blockingStacktrace = new StackTrace();

            lock (_sync)
            {
                // If we are already blocking, then posting to the synchronization
                // context will (potentially) block the operation.
                if (_isBlocking)
                    throw new AopDeadlockException(_caller, BaseSynchronizationContext != null);
            }

            SendOrPostCallback restoreContextCallback = (state2) =>
            {
                // Asp.Net resets the sychronization context, so we need to restore it ourselves.
                SetSynchronizationContext(this);
                d(state2);
            };

            // Post the actual completion method, so it will be executed
            BaseSynchronizationContext.Post(restoreContextCallback, state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (BaseSynchronizationContext != null)
                BaseSynchronizationContext.Send(d, state);
            else
                base.Send(d, state);
        }

        public override void OperationStarted()
        {
            if (BaseSynchronizationContext != null)
                BaseSynchronizationContext.OperationStarted();
            else
                base.OperationStarted();
        }

        public override void OperationCompleted()
        {
            if (BaseSynchronizationContext != null)
                BaseSynchronizationContext.OperationCompleted();
            else
                base.OperationCompleted();
        }

        [SecurityCritical]
        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            // If we are waiting from the current thread, we have a deadlock.
            if (_currentThread == Thread.CurrentThread)
            {
                throw new AopDeadlockException(_caller, BaseSynchronizationContext != null);
            }

            // We cannot block multiple times at once
            try
            {
                lock (_sync)
                {
                    _isBlocking = true;
                }

                var waitContext = BaseSynchronizationContext ?? this;
                return waitContext.Wait(waitHandles, waitAll, millisecondsTimeout);
            }
            finally
            {
                _isBlocking = false;
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            var copy = new DeadlockDetectionSynchronizationContext(BaseSynchronizationContext?.CreateCopy(),_caller);
            lock (_sync)
            {
                copy._isBlocking = _isBlocking;
            }

            return copy;
        }
    }

    /// <summary>
    /// Reports a (potential) deadlock situation.
    /// </summary>
    public class AopDeadlockException : Exception
    {
        /// <summary>
        /// Stack trace where the program initiated the blocking operation.
        /// </summary>
        public StackTrace BlockingStackTrace { get; }

        /// <summary>
        /// Flag indicating whether the deadlock is a potential deadlock or an
        /// actual deadlock situation.
        /// </summary>
        public bool IsPotentialDeadlock { get; }
        
        /// <summary>
        /// 内部错误信息
        /// </summary>
        public string InnerExceptionMsg { get; }

        internal AopDeadlockException(StackTrace blockingStackTrace, bool isPotentialDeadlock)
            : base(GetMessage(blockingStackTrace,null, isPotentialDeadlock))
        {
            BlockingStackTrace = blockingStackTrace;
            IsPotentialDeadlock = isPotentialDeadlock;
        }

        internal AopDeadlockException(string blockingStackTrace, bool isPotentialDeadlock)
            : base(GetMessage(null,blockingStackTrace, isPotentialDeadlock))
        {
            InnerExceptionMsg = blockingStackTrace;
            IsPotentialDeadlock = isPotentialDeadlock;
        }
        
        private static string GetMessage(StackTrace blockingStackTrace,string message, bool isPotentialDeadlock)
        {
            // Generate the proper message
            var sb = new StringBuilder();
            sb.AppendLine(isPotentialDeadlock ? "The blocking operation encountered a potential deadlock." : "The blocking operation encountered a deadlock.");

            // Append stack trace of the blocking point (if any)
            if (blockingStackTrace != null)
            {
                sb.AppendLine();
                sb.AppendLine("Stack trace where the blocking operation started:");
                sb.Append(blockingStackTrace);
            }
            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine();
                sb.AppendLine("Stack trace where the blocking operation started:");
                sb.Append(message);
            }
            // Return message
            return sb.ToString();
        }
    }
}