using System.Collections.Generic;
using System.Linq;
using Core.Objects;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class GraphVisitorTest
    {
        private static readonly Node node0 = new Node {Id = 0};
        private static readonly Node node1 = new Node { Id = 1, IsMine = true};
        private static readonly Node node2 = new Node {Id = 2};
        private static readonly Node node3 = new Node {Id = 3};
        private static readonly Node node4 = new Node {Id = 4};
        private static readonly Node node5 = new Node {Id = 5, IsMine = true};
        private static readonly Node node6 = new Node {Id = 6};
        private static readonly Node node7 = new Node {Id = 7};

        private static readonly Punter myPunter = new Punter {Id = 0};
        private static readonly Punter otherPunter = new Punter { Id = 1 };

        [Test]
        [TestCaseSource(nameof(GenTests))]
        public void TestGetReachableNodesForPunter(Node from, Map map, Punter punter, Node[] expected)
        {
            var graphVisitor = new GraphVisitor();
            var actual = graphVisitor.GetReachableNodesForPunter(@from, map, punter);
            actual.ShouldAllBeEquivalentTo(expected);
        }

        [Test]
        public void TestGetReachableNodesFromMinesForPunter()
        {
            var map = GetMapFromSampleA();
            var graphVisitor = new GraphVisitor();
            var actual = graphVisitor.GetReachableNodesFromMinesForPunter(map, myPunter);
            actual.ShouldAllBeEquivalentTo(new [] {node0, node1, node2, node3, node4, node5, node6, node7});
        }

        [Test]
        public void TestGetGetConnectedComponents()
        {
            var map = GetMapFromSampleA();
            var graphVisitor = new GraphVisitor();
            var actual = graphVisitor.GetConnectedComponents(map);

            var component1 = new[] {node0, node1, node2, node3};
            var component2 = new[] { node4, node5, node6, node7 };

            actual.GetComponent(myPunter.Id, node0.Id).ShouldAllBeEquivalentTo(component1.Select(x => x.Id));
            actual.GetComponent(myPunter.Id, node4.Id).ShouldAllBeEquivalentTo(component2.Select(x => x.Id));

            CheckComponents(component1, component1, actual, myPunter.Id, true);
            CheckComponents(component2, component2, actual, myPunter.Id, true);
            CheckComponents(component1, component2, actual, myPunter.Id, false);

            var component3 = new[] { node0, node1, node2, node7 };
            var component4 = new[] { node3, node4, node5, node6 };

            actual.GetComponent(otherPunter.Id, node0.Id).ShouldAllBeEquivalentTo(component3.Select(x => x.Id));
            actual.GetComponent(otherPunter.Id, node3.Id).ShouldAllBeEquivalentTo(component4.Select(x => x.Id));

            CheckComponents(component3, component3, actual, otherPunter.Id, true);
            CheckComponents(component4, component4, actual, otherPunter.Id, true);
            CheckComponents(component3, component4, actual, otherPunter.Id, false);
        }

        private static void CheckComponents(Node[] component1, Node[] component2, PunterConnectedComponents actual, int punterId, bool expected)
        {
            foreach (var left in component1)
            {
                foreach (var right in component2)
                {
                    actual.IsInSameComponent(left.Id, right.Id, punterId).Should().Be(expected, $"{left.Id} - {right.Id}, punter {punterId}");
                }
            }
        }

        private static IEnumerable<TestCaseData> GenTests()
        {
            yield return new TestCaseData(
                node0,
                new Map(new[] {node0}, new Edge[0]),
                myPunter,
                new[] {node0,}
            ).SetName("Пустой граф");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] {node0, node1},
                    new[] {new Edge {Source = node0, Target = node1}}
                ),
                myPunter,
                new[]
                {
                    node0,
                }
            ).SetName("Две вершины, одно ребро, ничейное");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] { node0, node1 },
                    new[] { new Edge { Source = node0, Target = node1, Punter = otherPunter} }
                ),
                myPunter,
                new[]
                {
                    node0,
                }
            ).SetName("Две вершины, одно ребро, другой пунтер");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] { node0, node1 },
                    new[] { new Edge { Source = node0, Target = node1, Punter = myPunter } }
                ),
                myPunter,
                new[]
                {
                    node0,
                    node1,
                }
            ).SetName("Две вершины, одно ребро, нужный пунтер");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] {node0, node1, node2, node3},
                    new[]
                    {
                        new Edge {Source = node0, Target = node1, Punter = myPunter},
                        new Edge {Source = node0, Target = node2, Punter = otherPunter},
                        new Edge {Source = node0, Target = node3, Punter = myPunter},
                        new Edge {Source = node1, Target = node2, Punter = myPunter},
                        new Edge {Source = node1, Target = node3, Punter = otherPunter},
                        new Edge {Source = node2, Target = node3, Punter = myPunter},
                    }
                ),
                myPunter,
                new[]
                {
                    node0,
                    node1,
                    node2,
                    node3
                }
            ).SetName("Полный граф на 4 вершины. Ребра по периметру - у нужного пунтера");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] { node0, node1, node2, node3 },
                    new[]
                    {
                        new Edge {Source = node0, Target = node1, Punter = otherPunter},
                        new Edge {Source = node0, Target = node2, Punter = myPunter},
                        new Edge {Source = node0, Target = node3, Punter = otherPunter},
                        new Edge {Source = node1, Target = node2, Punter = otherPunter},
                        new Edge {Source = node1, Target = node3, Punter = myPunter},
                        new Edge {Source = node2, Target = node3, Punter = otherPunter},
                    }
                ),
                myPunter,
                new[]
                {
                    node0,
                    node2,
                }
            ).SetName("Полный граф на 4 вершины. Ребра по диагонали - у нужного пунтера");

            yield return new TestCaseData(
                node0,
                GetMapFromSampleA(),
                myPunter,
                new[]
                {
                    node0,
                    node1,
                    node2,
                    node3
                }
            ).SetName("Сэмпл А из условия. Играем за желтого из вершины 0");

            yield return new TestCaseData(
                node7,
                GetMapFromSampleA(),
                myPunter,
                new[]
                {
                    node7,
                    node6,
                    node5,
                    node4
                }
            ).SetName("Сэмпл А из условия. Играем за желтого из вершины 7");
        }

        private static Map GetMapFromSampleA()
        {
            return new Map(
                new[] {node0, node1, node2, node3, node4, node5, node6, node7, new Node {Id = 666}},
                new[]
                {
                    new Edge {Source = node0, Target = node1, Punter = myPunter},
                    new Edge {Source = node1, Target = node2, Punter = otherPunter},
                    new Edge {Source = node2, Target = node3, Punter = myPunter},
                    new Edge {Source = node3, Target = node4, Punter = otherPunter},
                    new Edge {Source = node4, Target = node5, Punter = myPunter},
                    new Edge {Source = node5, Target = node6, Punter = otherPunter},
                    new Edge {Source = node6, Target = node7, Punter = myPunter},
                    new Edge {Source = node7, Target = node0, Punter = otherPunter},

                    new Edge {Source = node1, Target = node3, Punter = myPunter},
                    new Edge {Source = node3, Target = node5, Punter = otherPunter},
                    new Edge {Source = node5, Target = node7, Punter = myPunter},
                    new Edge {Source = node7, Target = node1, Punter = otherPunter},
                }
            );
        }
    }
}