﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LanguageExt;
using static LanguageExt.Prelude;
using static LanguageExt.Parsec.Char;
using static LanguageExt.Parsec.Expr;
using static LanguageExt.Parsec.Prim;
using static LanguageExt.Parsec.Token;
using static LanguageExt.Parsec.Indent;
using LanguageExt.Parsec;
using LanguageExt.UnitsOfMeasure;

namespace Echo.Config
{
    public class Types
    {
        public readonly TypeDef Bool;
        public readonly TypeDef Int;
        public readonly TypeDef Double;
        public readonly TypeDef String;
        public readonly TypeDef ProcessId;
        public readonly TypeDef ProcessName;
        public readonly TypeDef ProcessFlags;
        public readonly TypeDef Time;
        public readonly TypeDef MessageDirective;
        public readonly TypeDef Directive;
        public readonly TypeDef DispatcherType;

        public readonly TypeDef Object;

        public HashMap<string, TypeDef> TypeMap
        {
            get;
            private set;
        }

        public HashMap<string, TypeDef> All
        {
            get;
            private set;
        }

        public Seq<TypeDef> AllInOrder
        {
            get;
            private set;
        }

        public Types()
        {
            Int = new TypeDef(
                "int",
                (_,x) => x,
                typeof(int),
                p => from x in p.integer
                     select (object)x,
                HashMap(
                    OpT("+", () => Int, (lhs, rhs) => (int)lhs + (int)rhs),
                    OpT("-", () => Int, (lhs, rhs) => (int)lhs - (int)rhs),
                    OpT("*", () => Int, (lhs, rhs) => (int)lhs * (int)rhs),
                    OpT("/", () => Int, (lhs, rhs) => (int)lhs / (int)rhs),
                    OpT("%", () => Int, (lhs, rhs) => (int)lhs % (int)rhs),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value != (int)rhs.Value)),
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value == (int)rhs.Value)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value < (int)rhs.Value)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value <= (int)rhs.Value)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value > (int)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (int)lhs.Value >= (int)rhs.Value)),
                    OpT("^", () => Int, (lhs, rhs) => Math.Pow((int)lhs, (int)rhs)),
                    OpT("&", () => String, (lhs, rhs) => ((int)lhs & (int)rhs)),
                    OpT("|", () => String, (lhs, rhs) => ((int)lhs | (int)rhs))
                ),
                HashMap(
                    OpT("+", () => Int, rhs => +(int)rhs),
                    OpT("-", () => Int, rhs => -(int)rhs)
                ),
                null,
                HashMap(
                    Conv("float", obj => (int)((double)obj)),
                    Conv("process-flags", obj => (int)((ProcessFlags)obj))
                ),
                null,
                2
            );

            Double = new TypeDef(
                "float",
                (_, x) => x,
                typeof(double),
                p => from x in p.floating
                     select (object)x,
                HashMap(
                    OpT("+", () => Int, (lhs, rhs) => (double)lhs + (double)rhs),
                    OpT("-", () => Int, (lhs, rhs) => (double)lhs - (double)rhs),
                    OpT("*", () => Int, (lhs, rhs) => (double)lhs * (double)rhs),
                    OpT("/", () => Int, (lhs, rhs) => (double)lhs / (double)rhs),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value != (double)rhs.Value)),
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value == (double)rhs.Value)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value < (double)rhs.Value)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value <= (double)rhs.Value)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value > (double)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (double)lhs.Value >= (double)rhs.Value)),
                    OpT("^", () => Int, (lhs, rhs) => Math.Pow((double)lhs, (double)rhs))
                ),
                HashMap(
                    OpT("+", () => Int, rhs => +(double)rhs),
                    OpT("-", () => Int, rhs => -(double)rhs)
                ),
                null,
                HashMap(
                    Conv("int", obj => (double)((int)obj))
                ),
                null,
                1
            );

            Bool = new TypeDef(
                "bool",
                (_, x) => x,
                typeof(bool),
                p => from v in choice(
                        p.reserved("true"),
                        p.reserved("false"),
                        p.reserved("yes"),
                        p.reserved("no")
                        )
                     select (object)(v == "true" || v == "yes"),
                HashMap(
                    OpT("!=", () => Bool, (lhs, rhs) => (bool)lhs != (bool)rhs),
                    OpT("==", () => Bool, (lhs, rhs) => (bool)lhs == (bool)rhs),
                    OpT("&&", () => Bool, (lhs, rhs) => (bool)lhs && (bool)rhs),
                    OpT("||", () => Bool, (lhs, rhs) => (bool)lhs || (bool)rhs)
                ),
                HashMap(
                    OpT("!", () => Int, rhs => !(bool)rhs)
                ),
                null,
                null,
                null,
                0
            );

            String = new TypeDef(
                "string",
                (_, x) => x,
                typeof(string),
                p => from _ in unitp
                     from v in p.stringLiteral
                     select (object)v,
                HashMap(
                    OpT("+", () => String, (lhs, rhs) => (string)lhs + (string)rhs),
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (string)lhs.Value == (string)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (string)lhs.Value != (string)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((string)lhs.Value).CompareTo((string)rhs.Value) >= 0)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((string)lhs.Value).CompareTo((string)rhs.Value) <= 0)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((string)lhs.Value).CompareTo((string)rhs.Value) > 0)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((string)lhs.Value).CompareTo((string)rhs.Value) < 0))
                ),
                HashMap(
                    OpT("!", () => Int, rhs => !(bool)rhs)
                ),
                null,
                HashMap(
                    Conv("int", obj => obj.ToString()),
                    Conv("float", obj => obj.ToString()),
                    Conv("bool", obj => obj.ToString()),
                    Conv("process-id", obj => obj.ToString()),
                    Conv("process-name", obj => obj.ToString()),
                    Conv("process-flags", obj => obj.ToString()),
                    Conv("time", obj => obj.ToString()),
                    Conv("directive", obj => obj.ToString()),
                    Conv("message-directive", obj => obj.ToString()),
                    Conv("disp", obj => obj.ToString())
                ),
                null,
                0
            );

            ProcessId = new TypeDef(
                "process-id",
                (_, x) => x,
                typeof(ProcessId),
                p => from v in p.processId
                     select (object)v,
                HashMap(
                    OpT("+", () => String, (lhs, rhs) => ((ProcessId)lhs).Append((ProcessId)rhs)),
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessId)lhs.Value == (ProcessId)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessId)lhs.Value != (ProcessId)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessId)lhs.Value).CompareTo((ProcessId)rhs.Value) >= 0)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessId)lhs.Value).CompareTo((ProcessId)rhs.Value) <= 0)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessId)lhs.Value).CompareTo((ProcessId)rhs.Value) > 0)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessId)lhs.Value).CompareTo((ProcessId)rhs.Value) < 0))
                ),
                null,
                null,
                HashMap(
                    Conv("process-name", obj => new ProcessId("/"+((ProcessName)obj).Value)),
                    Conv("string", obj => new ProcessId((string)obj))
                ),
                null,
                10
            );

            ProcessName = new TypeDef(
                "process-name",
                (_, x) => x,
                typeof(ProcessName),
                p => from _ in unitp
                     from v in p.processName
                     select (object)v,
                HashMap(
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessName)lhs.Value == (ProcessName)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessName)lhs.Value != (ProcessName)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessName)lhs.Value).CompareTo((ProcessName)rhs.Value) >= 0)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessName)lhs.Value).CompareTo((ProcessName)rhs.Value) <= 0)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessName)lhs.Value).CompareTo((ProcessName)rhs.Value) > 0)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessName)lhs.Value).CompareTo((ProcessName)rhs.Value) < 0))
                ),
                null,
                null,
                HashMap(
                    Conv("string", obj => new ProcessName((string)obj))
                ),
                null,
                10
            );

            Func<ProcessSystemConfigParser, string, ProcessFlags, Parser<ProcessFlags>> flagMap =
                (p, name, flags) =>
                    attempt(
                    from x in p.reserved(name)
                    select flags);

            Func<ProcessSystemConfigParser, Parser<ProcessFlags>> flag =
                p =>
                    choice(
                        flagMap(p, "default", Echo.ProcessFlags.Default),
                        flagMap(p, "listen-remote-and-local", Echo.ProcessFlags.ListenRemoteAndLocal),
                        flagMap(p, "persist-all", Echo.ProcessFlags.PersistAll),
                        flagMap(p, "persist-inbox", Echo.ProcessFlags.PersistInbox),
                        flagMap(p, "persist-state", Echo.ProcessFlags.PersistState),
                        flagMap(p, "remote-publish", Echo.ProcessFlags.RemotePublish),
                        flagMap(p, "remote-state-publish", Echo.ProcessFlags.RemoteStatePublish));

            Func<ProcessSystemConfigParser, Parser<ProcessFlags>> flagsValue =
                p =>
                    from fs in p.brackets(p.commaSep(flag(p)))
                    select fs.Fold(Echo.ProcessFlags.Default, (s, x) => s | x);

            ProcessFlags = new TypeDef(
                "process-flags",
                (_, x) => x,
                typeof(ProcessFlags),
                p => from v in flagsValue(p)
                     select (object)v,
                HashMap(
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessFlags)lhs.Value == (ProcessFlags)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (ProcessFlags)lhs.Value != (ProcessFlags)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessFlags)lhs.Value).CompareTo((ProcessFlags)rhs.Value) >= 0)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessFlags)lhs.Value).CompareTo((ProcessFlags)rhs.Value) <= 0)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessFlags)lhs.Value).CompareTo((ProcessFlags)rhs.Value) > 0)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((ProcessFlags)lhs.Value).CompareTo((ProcessFlags)rhs.Value) < 0)),
                    OpT("&", () => String, (lhs, rhs) => ((ProcessFlags)lhs & (ProcessFlags)rhs)),
                    OpT("|", () => String, (lhs, rhs) => ((ProcessFlags)lhs | (ProcessFlags)rhs))
                ),
                HashMap(
                    OpT("~", () => String, rhs => ~(ProcessFlags)rhs)
                ),
                null,
                HashMap(
                    Conv("int", obj => (ProcessFlags)((int)obj))
                ),
                null,
                10
            );

            Func<ProcessSystemConfigParser, Parser<string>> timeUnit =
                p =>
                    choice(
                        attempt(p.reserved("seconds")),
                        attempt(p.reserved("second")),
                        attempt(p.reserved("secs")),
                        attempt(p.reserved("sec")),
                        attempt(p.reserved("s")),
                        attempt(p.reserved("minutes")),
                        attempt(p.reserved("minute")),
                        attempt(p.reserved("mins")),
                        attempt(p.reserved("min")),
                        attempt(p.reserved("milliseconds")),
                        attempt(p.reserved("millisecond")),
                        attempt(p.reserved("ms")),
                        attempt(p.reserved("hours")),
                        attempt(p.reserved("hour")),
                        p.reserved("hr"))
                       .label("Unit of time (e.g. seconds, mins, hours, hr, sec, min...)");

            Func<ProcessSystemConfigParser, Parser<Time>> timeValue =
                p =>
                    from v in p.floating
                    from u in timeUnit(p)
                    from r in TimeAttr.TryParse(v, u).Match(
                        Some: result,
                        None: () => failure<Time>("Invalid unit of time"))
                    select r;

            Time = new TypeDef(
                "time",
                (_, x) => x,
                typeof(Time),
                p => from v in p.token(timeValue(p))
                     select (object)v,
                HashMap(
                    OpT("+", () => String, (lhs, rhs) => ((Time)lhs + (Time)rhs)),
                    OpT("-", () => String, (lhs, rhs) => ((Time)lhs - (Time)rhs)),
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (Time)lhs.Value == (Time)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (Time)lhs.Value != (Time)rhs.Value)),
                    Op(">=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((Time)lhs.Value).CompareTo((Time)rhs.Value) >= 0)),
                    Op("<=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((Time)lhs.Value).CompareTo((Time)rhs.Value) <= 0)),
                    Op(">", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((Time)lhs.Value).CompareTo((Time)rhs.Value) > 0)),
                    Op("<", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, ((Time)lhs.Value).CompareTo((Time)rhs.Value) < 0))
                ),
                HashMap(
                    OpT("+", () => Int, rhs => 0.Seconds() + (Time)rhs),
                    OpT("-", () => Int, rhs => 0.Seconds() - (Time)rhs)
                ),
                null,
                null,
                null,
                0
            );

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> fwdToSelf =
                p =>
                    from _ in p.reserved("forward-to-self")
                    select new ForwardToSelf() as MessageDirective;

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> fwdToParent =
                p =>
                    from _ in p.reserved("forward-to-parent")
                    select new ForwardToParent() as MessageDirective;

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> fwdToDeadLetters =
                p =>
                    from _ in p.reserved("forward-to-dead-letters")
                    select new ForwardToDeadLetters() as MessageDirective;

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> stayInQueue =
                p =>
                    from _ in p.reserved("stay-in-queue")
                    select new StayInQueue() as MessageDirective;

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> fwdToProcess =
                p =>
                    from _ in p.reserved("forward-to-process")
                    from pid in attempt(p.expr(None, ProcessId)).label("'forward-to-process <ProcessId>'")
                    select new ForwardToProcess((ProcessId)pid.Value) as MessageDirective;

            Func<ProcessSystemConfigParser, Parser<MessageDirective>> msgDirective =
                p =>
                    choice(
                        fwdToDeadLetters(p),
                        fwdToSelf(p),
                        fwdToParent(p),
                        fwdToProcess(p),
                        stayInQueue(p));

            MessageDirective = new TypeDef(
                "message-directive",
                (_, x) => x,
                typeof(MessageDirective),
                p => from v in p.token(msgDirective(p))
                     select (object)v,
                HashMap(
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (MessageDirective)lhs.Value == (MessageDirective)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (MessageDirective)lhs.Value != (MessageDirective)rhs.Value))
                ),
                null,
                null,
                null,
                null,
                10
            );

            Func<ProcessSystemConfigParser, Parser<Directive>> directive =
                p =>
                    choice(
                        p.reserved("resume").Map(_ => Echo.Directive.Resume),
                        p.reserved("restart").Map(_ => Echo.Directive.Restart),
                        p.reserved("stop").Map(_ => Echo.Directive.Stop),
                        p.reserved("escalate").Map(_ => Echo.Directive.Escalate));

            Directive = new TypeDef(
                "directive",
                (_, x) => x,
                typeof(Directive),
                p => from v in p.token(directive(p))
                     select (object)v,
                HashMap(
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (Directive)lhs.Value == (Directive)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (Directive)lhs.Value != (Directive)rhs.Value))
                ),
                null,
                null,
                null,
                null,
                10
            );

            Func<ProcessSystemConfigParser, Parser<string>> dispType =
                p =>
                    choice(
                        p.reserved("broadcast"),
                        attempt(p.reserved("least-busy")),
                        attempt(p.reserved("round-robin")),
                        p.reserved("random"),
                        p.reserved("hash"),
                        p.reserved("first"),
                        p.reserved("second"),
                        p.reserved("third"),
                        p.reserved("last")
                    );

            DispatcherType = new TypeDef(
                "disp",
                (_, x) => x,
                typeof(string),
                p => from v in p.token(dispType(p))
                     select (object)v,
                HashMap(
                    Op("==", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (string)lhs.Value == (string)rhs.Value)),
                    Op("!=", (ValueToken lhs, ValueToken rhs) => new ValueToken(Bool, (string)lhs.Value != (string)rhs.Value))
                ),
                null,
                null,
                null,
                null,
                10
            );

            TypeMap = HashMap(
                (typeof(bool).FullName, Bool),
                (typeof(int).FullName, Int),
                (typeof(double).FullName, Double),
                (typeof(string).FullName, String),
                (typeof(ProcessId).FullName, ProcessId),
                (typeof(ProcessFlags).FullName, ProcessFlags),
                (typeof(Time).FullName, Time),
                (typeof(MessageDirective).FullName, MessageDirective),
                (typeof(Directive).FullName, Directive)
            );

            All = HashMap(
                (Bool.Name, Bool),
                (Int.Name, Int),
                (Double.Name, Double),
                (String.Name, String),
                (ProcessId.Name, ProcessId),
                (ProcessFlags.Name, ProcessFlags),
                (Time.Name, Time),
                (MessageDirective.Name, MessageDirective),
                (Directive.Name, Directive),
                (DispatcherType.Name, DispatcherType)
                );
            AllInOrder = (from x in All.Values
                          orderby x.Order descending
                          select x)
                         .ToSeq()
                         .Strict();
        }

        public bool Exists(Type type) =>
            TypeMap.ContainsKey(type.FullName);

        public TypeDef Get(Type type) =>
            TypeMap[type.FullName];

        public TypeDef Get(string name) =>
            All[name];

        public TypeDef Register(TypeDef type)
        {
            All = All.Add(type.Name, type);
            AllInOrder = (from x in All.Values
                          orderby x.Order descending
                          select x)
                         .ToSeq()
                         .Strict();
            return type;
        }

        public Unit MapTo(TypeDef def)
        {
            TypeMap = TypeMap.Add(def.MapsTo.FullName, def);
            return unit;
        }

        public static Tuple<string,Func<object, object>> Conv(string fromType, Func<object, object> conv) =>
            Tuple(fromType, conv);

        public static Tuple<string, Func<ValueToken, ValueToken, ValueToken>> Op(string name, Func<ValueToken, ValueToken, ValueToken> op) =>
            Tuple(name, op);

        public static Tuple<string, Func<ValueToken, ValueToken, ValueToken>> OpT(string name, Func<TypeDef> type, Func<object, object, object> op) =>
            Tuple<string, Func<ValueToken, ValueToken, ValueToken>>(name, (ValueToken lhs, ValueToken rhs) => new ValueToken(type(), op(lhs.Value,rhs.Value)));

        public static Tuple<string, Func<ValueToken, ValueToken>> Op(string name, Func<ValueToken, ValueToken> op) =>
            Tuple(name, op);

        public static Tuple<string, Func<ValueToken, ValueToken>> OpT(string name, Func<TypeDef> type, Func<object, object> op) =>
            Tuple<string, Func<ValueToken, ValueToken>>(name, (ValueToken tok) => new ValueToken(type(), op(tok.Value)));
    }
}
