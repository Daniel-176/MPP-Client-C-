using System;
using System.Collections.Generic;

namespace MPP_Client_C_
{
    public class EventEmitter
    {
        private readonly Dictionary<string, Action<object[]>> _events = new Dictionary<string, Action<object[]>>();

        public void On(string evnt, Action<object[]> fn)
        {
            if (!this._events.ContainsKey(evnt))
                this._events.Add(evnt, (Action<object[]>)(o => { }));
            this._events[evnt] += fn;
        }

        public void Off(string evnt, Action<object[]> fn)
        {
            if (!this._events.ContainsKey(evnt))
                return;
            this._events[evnt] -= fn;
        }

        public void Emit(string evnt, params object[] arguments)
        {
            if (!this._events.ContainsKey(evnt))
                return;
            this._events[evnt](arguments);
        }
    }
}
