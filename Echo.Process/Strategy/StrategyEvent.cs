﻿using LanguageExt;
using System;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Echo
{
    public static class StrategyEvent
    {
        /// <summary>
        /// Creates a new State computation that is primed with the data of a particular
        /// failure event.  
        /// </summary>
        /// <param name="strategy">Strategy as a State computation</param>
        /// <param name="pid">Process ID that failed</param>
        /// <param name="sender">Process that sent the message that cause the failure</param>
        /// <param name="parent">Supervisor of the failed Process</param>
        /// <param name="siblings">The siblings of the failed Process</param>
        /// <param name="ex">Exception</param>
        /// <param name="msg">Message that caused the failure</param>
        /// <returns>State computation that can be invoked by passing it
        /// an object of StrategyState.  This will result in a StateResult that contains
        /// the mutated StrategyState and a StrategyDecision.  The StrategyDecision 
        /// contains all the information needed to decide the fate of a Process (and
        /// related processes)</returns>
        public static State<StrategyState, StrategyDecision> Failure(
            this State<StrategyContext, Unit> strategy,
            ProcessId pid,
            ProcessId sender,
            ProcessId parent,
            IEnumerable<ProcessId> siblings,
            Exception ex,
            object msg ) =>
                State<StrategyState, StrategyDecision>(stateInst =>
                {
                    var now = DateTime.UtcNow;

                    var state = strategy.Run(
                        StrategyContext.Empty.With(
                            Global: stateInst.With(Failures: stateInst.Failures + 1),
                            FailedProcess: pid,
                            ParentProcess: parent,
                            Sender: sender,
                            Siblings: siblings,
                            Exception: ex,
                            Message: msg)).State;

                    var decision = new StrategyDecision(
                        state.Directive.IfNone(Directive.Restart),
                        state.MessageDirective.IfNone(MessageDirective.ForwardToDeadLetters),
                        state.Affects,
                        state.Pause
                    );

                    return (decision, state.Global.With(FirstFailure: stateInst.FirstFailure == DateTime.MaxValue ? DateTime.UtcNow : stateInst.FirstFailure, LastFailure: now));
                });
    }
}
