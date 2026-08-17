using System.Collections.Generic;

namespace ImpossibleRobert.Common {
    public class AhoCorasick {
        public class Node {
            public readonly Dictionary<byte, Node> Children = new Dictionary<byte, Node>();
            public Node Fail;
            public readonly List<int> Outputs = new List<int>();
        }

        private readonly Node _root = new Node();

        public AhoCorasick(IList<byte[]> patterns) {
            for (var i = 0; i < patterns.Count; i++) {
                var pat = patterns[i];
                var node = _root;
                foreach (var b in pat) {
                    if (!node.Children.TryGetValue(b, out var next)) {
                        next = new Node();
                        node.Children[b] = next;
                    }
                    node = next;
                }
                node.Outputs.Add(i);
            }

            var q = new Queue<Node>();
            foreach (var child in _root.Children.Values) {
                child.Fail = _root;
                q.Enqueue(child);
            }

            while (q.Count > 0) {
                var current = q.Dequeue();
                foreach (var kv in current.Children) {
                    var transition = kv.Key;
                    var childNode = kv.Value;
                    var failNode = current.Fail;
                    Node nextFail = null;

                    while (failNode != null && !failNode.Children.TryGetValue(transition, out nextFail)) {
                        failNode = failNode.Fail;
                    }

                    childNode.Fail = nextFail ?? _root;
                    childNode.Outputs.AddRange(childNode.Fail.Outputs);
                    q.Enqueue(childNode);
                }
            }
        }

        public void Scan(byte[] buffer, int length, HashSet<int> foundIds, ref Node state) {
            for (var i = 0; i < length; i++) {
                var b = buffer[i];
                while (state != _root && !state.Children.ContainsKey(b)) {
                    state = state.Fail;
                }
                if (state.Children.TryGetValue(b, out var next)) {
                    state = next;
                    foreach (var id in state.Outputs) {
                        foundIds.Add(id);
                    }
                }
            }
        }

        public Node Root => _root;
    }
}
