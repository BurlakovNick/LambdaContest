using System.Collections.Generic;
using Core.Objects;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class DistanceCalculatorTest
    {
        [Test]
        [TestCaseSource(nameof(GenTests))]
        public void TestGetShortest(Node from, Map map, ShortestDistance[] expected)
        {
            var distanceCalculator = new DistanceCalculator();
            var actual = distanceCalculator.GetShortest(@from, map);
            actual.ShouldAllBeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenTests()
        {
            var node0 = new Node {Id = 0};
            var node1 = new Node {Id = 1};
            var node2 = new Node { Id = 2 };
            var node3 = new Node { Id = 3 };
            var node4 = new Node { Id = 4 };
            var node5 = new Node { Id = 5 };
            var node6 = new Node { Id = 6 };
            var node7 = new Node { Id = 7 };

            yield return new TestCaseData(
                node0,
                new Map(new[] { node0}, new Edge[0]),
                new[] { new ShortestDistance{From = node0, To = node0, Length = 0}, }
                ).SetName("Пустой граф");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] {node0, node1},
                    new[] {new Edge {Source = node0, Target = node1}}
                ),
                new[]
                {
                    new ShortestDistance {From = node0, To = node0, Length = 0},
                    new ShortestDistance {From = node0, To = node1, Length = 1},
                }
            ).SetName("Две вершины, одно ребро");

            yield return new TestCaseData(
                node0,
                new Map(
                    new[] { node0, node1, node2, node3 },
                    new[]
                    {
                        new Edge { Source = node0, Target = node1 },
                        new Edge { Source = node0, Target = node2 },
                        new Edge { Source = node0, Target = node3 },
                        new Edge { Source = node1, Target = node2 },
                        new Edge { Source = node1, Target = node3 },
                        new Edge { Source = node2, Target = node3 },
                    }
                ),
                new[]
                {
                    new ShortestDistance {From = node0, To = node0, Length = 0},
                    new ShortestDistance {From = node0, To = node1, Length = 1},
                    new ShortestDistance {From = node0, To = node2, Length = 1},
                    new ShortestDistance {From = node0, To = node3, Length = 1},
                }
            ).SetName("Полный граф на 4 вершины");

            yield return new TestCaseData(
                node0,
                GetMapFromSampleA(node0, node1, node2, node3, node4, node5, node6, node7),
                new[]
                {
                    new ShortestDistance {From = node0, To = node0, Length = 0},
                    new ShortestDistance {From = node0, To = node1, Length = 1},
                    new ShortestDistance {From = node0, To = node2, Length = 2},
                    new ShortestDistance {From = node0, To = node3, Length = 2},
                    new ShortestDistance {From = node0, To = node4, Length = 3},
                    new ShortestDistance {From = node0, To = node5, Length = 2},
                    new ShortestDistance {From = node0, To = node6, Length = 2},
                    new ShortestDistance {From = node0, To = node7, Length = 1},
                }
            ).SetName("Сэмпл А из условия. Научинаем из вершины 0. Плюс недостижимая вершина");

            yield return new TestCaseData(
                node3,
                GetMapFromSampleA(node0, node1, node2, node3, node4, node5, node6, node7),
                new[]
                {
                    new ShortestDistance {From = node3, To = node0, Length = 2},
                    new ShortestDistance {From = node3, To = node1, Length = 1},
                    new ShortestDistance {From = node3, To = node2, Length = 1},
                    new ShortestDistance {From = node3, To = node3, Length = 0},
                    new ShortestDistance {From = node3, To = node4, Length = 1},
                    new ShortestDistance {From = node3, To = node5, Length = 1},
                    new ShortestDistance {From = node3, To = node6, Length = 2},
                    new ShortestDistance {From = node3, To = node7, Length = 2},
                }
            ).SetName("Сэмпл А из условия. Научинаем из вершины 3. Плюс недостижимая вершина");
        }

        private static Map GetMapFromSampleA(Node node0, Node node1, Node node2, Node node3, Node node4, Node node5, Node node6, Node node7)
        {
            return new Map(
                new[] { node0, node1, node2, node3, node4, node5, node6, node7, new Node{Id = 666} },
                new[]
                {
                    new Edge { Source = node0, Target = node1 },
                    new Edge { Source = node1, Target = node2 },
                    new Edge { Source = node2, Target = node3 },
                    new Edge { Source = node3, Target = node4 },
                    new Edge { Source = node4, Target = node5 },
                    new Edge { Source = node5, Target = node6 },
                    new Edge { Source = node6, Target = node7 },
                    new Edge { Source = node7, Target = node0 },

                    new Edge { Source = node1, Target = node3 },
                    new Edge { Source = node3, Target = node5 },
                    new Edge { Source = node5, Target = node7 },
                    new Edge { Source = node7, Target = node1 },
                }
            );
        }
    }
}