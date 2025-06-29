using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoG {

    public static class Shared<T> where T : new() {
        public static readonly T shared = new();
    }
}