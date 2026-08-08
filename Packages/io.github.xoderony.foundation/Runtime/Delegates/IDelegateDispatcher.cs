using System;

namespace Xoderony {

    public interface IDelegateDispatcher<TDelegate> where TDelegate : Delegate {

        TDelegate Handlers { get; }
    }
}
