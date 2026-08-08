using System;

namespace Xoderony {

    public sealed class DelegateChannel<TDelegate> : IDelegateSubscriber<TDelegate>, IDelegateDispatcher<TDelegate> where TDelegate : Delegate {

        private TDelegate _handlers;

        TDelegate IDelegateDispatcher<TDelegate>.Handlers => _handlers;

        void IDelegateSubscriber<TDelegate>.Subscribe(TDelegate handler) {
            _handlers = (TDelegate)Delegate.Combine(_handlers, handler);
        }

        void IDelegateSubscriber<TDelegate>.Unsubscribe(TDelegate handler) {
            _handlers = (TDelegate)Delegate.Remove(_handlers, handler);
        }
    }
}
