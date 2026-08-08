using System;

namespace Xoderony {

    public interface IDelegateSubscriber<TDelegate> where TDelegate : Delegate {

        void Subscribe(TDelegate handler);

        void Unsubscribe(TDelegate handler);

    }
}
