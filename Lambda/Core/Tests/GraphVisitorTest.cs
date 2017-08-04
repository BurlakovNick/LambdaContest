using System.Collections.Generic;
using Core.Objects;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class GraphVisitorTest
    {
        [Test]
        [TestCaseSource(nameof(GenTests))]
        public void TestGetReachableNodesForPunter(Node from, Map map, Punter punter, Node[] expected)
        {
            var graphVisitor = new GraphVisitor();
            var actual = graphVisitor.GetReachableNodesForPunter(@from, map, punter);
            actual.ShouldAllBeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenTests()
        {
            var myPunter = new Punter { Id = 0};
            var otherPunter = new Punter { Id = 1 };

            var node0 = new Node {Id = 0};
            var node1 = new Node {Id = 1};
            var node2 = new Node {Id = 2};
            var node3 = new Node {Id = 3};
            var node4 = new Node {Id = 4};
            var node5 = new Node {Id = 5};
            var node6 = new Node {Id = 6};
            var node7 = new Node {Id = 7};

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
                GetMapFromSampleA(node0, node1, node2, node3, node4, node5, node6, node7, myPunter, otherPunter),
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
                GetMapFromSampleA(node0, node1, node2, node3, node4, node5, node6, node7, myPunter, otherPunter),
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

        private static Map GetMapFromSampleA(Node node0, Node node1, Node node2, Node node3, Node node4, Node node5,
            Node node6, Node node7, Punter myPunter, Punter otherPunter)
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