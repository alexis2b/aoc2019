using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace aoc2019
{
    internal class Day06
    {
        public static void Run()
        {
            var input = File.ReadAllLines(Path.Combine("input", "day06.txt")).ToList();

            var res1 = OrbitChecksum(input);
            Console.WriteLine($"Day06 - part1 - result: {res1}");

            var res2 = OrbitTransfers(input, "YOU", "SAN");
            Console.WriteLine($"Day02 - part2 - result: {res2}");
        }

        public static int OrbitChecksum(IEnumerable<string> input)
        {
            var orbits = input.Select(s => s.Split(')')).Select(s => (c: s[0], o: s[1]));
            var orbitTree = BuildOrbitTree(orbits);

            var orbitCount = 0;
            foreach(var node in orbitTree.Values)
                orbitCount += Depth(node);
            return orbitCount;
        }

        public static int OrbitTransfers(IEnumerable<string> input, string from, string to)
        {
            var orbits = input.Select(s => s.Split(')')).Select(s => (c: s[0], o: s[1]));
            var orbitTree = BuildOrbitTree(orbits);
            var startNode = orbitTree[from];
            var endNode   = orbitTree[to];
            var visitedNodes = new HashSet<OrbitNode>();
            var frontier     = new List<OrbitNode>() { startNode };
            var distance     = 0;
            // Perform a breadth-first search until we reach "to"
            while(! frontier.Contains(endNode) )
            {
                frontier = frontier.SelectMany(n => n.Connections).Where(n => visitedNodes.Add(n)).ToList();
                distance++;
            }
            return distance - 2; // -2 because we ignore the orbit segments of YOU and SAN
        }

        // the Depth of a node is the number of parents it takes to reach the Center of Mass
        private static int Depth(OrbitNode node)
        {
            var depth = 0;
            for(var current = node; !current.IsCenterOfMass; current = current.Parent, depth++);
            return depth;
        }

        private static Dictionary<string, OrbitNode> BuildOrbitTree(IEnumerable<(string c, string o)> orbits)
        {
            var nodes = new Dictionary<string, OrbitNode>();

            // small helper function
            OrbitNode FindOrCreateNode(string id)
            {
                if ( ! nodes.TryGetValue(id, out OrbitNode node) )
                    return nodes[id] = new OrbitNode(id);
                return node;
            }

            foreach(var orbit in orbits)
            {
                var nodeC = FindOrCreateNode(orbit.c);
                var nodeO = FindOrCreateNode(orbit.o);
                nodeO.SetParent(nodeC);
            }

            return nodes;
        }


        private sealed class OrbitNode : IEquatable<OrbitNode>
        {
            private readonly HashSet<OrbitNode> _children = new HashSet<OrbitNode>();

            public string Id { get; private set; }
            public bool IsCenterOfMass => Id == "COM";
            public OrbitNode Parent { get; private set; }
            public IEnumerable<OrbitNode> Children => _children;

            public IEnumerable<OrbitNode> Connections => IsCenterOfMass ? _children : _children.Append(Parent);

            public OrbitNode(string id)
            {
                Id = id;
            }

            // Set the Parent of this node to the given parent node
            // also adds this node to the Children list of the parent
            public void SetParent(OrbitNode parent)
            {
                if ( IsCenterOfMass )
                    throw new Exception("Center of Mass can not have a parent");
                if ( Parent != null && Parent.Id != parent.Id )
                    throw new Exception($"Parent redefinition: {parent.Id} but was already {Parent.Id}");
                Parent = parent;
                parent._children.Add(this);
            }

            public override bool Equals(object obj) => Equals(obj as OrbitNode);

            public override int GetHashCode() => Id.GetHashCode();

            public bool Equals(OrbitNode other) => other != null && other.Id == Id;

            public override string ToString() => Id;
        }
    }


    [TestFixture]
    internal class Day06Tests
    {
        private static string[] TestMap1 = new[] {
            "COM)B",
            "B)C",
            "C)D",
            "D)E",
            "E)F",
            "B)G",
            "G)H",
            "D)I",
            "E)J",
            "J)K",
            "K)L",
        };

        private static string[] TestMap2 = new[] {
            "COM)B",
            "B)C",
            "C)D",
            "D)E",
            "E)F",
            "B)G",
            "G)H",
            "D)I",
            "E)J",
            "J)K",
            "K)L",
            "K)YOU",
            "I)SAN"
        };

        [Test]
        public void Test1()
        {
            var actual = Day06.OrbitChecksum(TestMap1);
            Assert.AreEqual(42, actual);
        }


        [Test]
        public void Test2()
        {
            var actual = Day06.OrbitTransfers(TestMap2, "YOU", "SAN");
            Assert.AreEqual(4, actual);
        }
    }
}
