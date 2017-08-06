using Core.Objects;
using FluentAssertions;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class ScorerTest
    {
        private Node node0;
        private Node node1;
        private Node node2;
        private Node node3;
        private Node node4;
        private Node node5;
        private Node node6;
        private Node node7;
        private Punter yellowPunter;
        private Punter redPunter;

        [SetUp]
        public void SetUp()
        {
            node0 = new Node { Id = 0 };
            node1 = new Node { Id = 1, IsMine = true};
            node2 = new Node { Id = 2 };
            node3 = new Node { Id = 3 };
            node4 = new Node { Id = 4 };
            node5 = new Node { Id = 5 };
            node6 = new Node { Id = 6, IsMine = true};
            node7 = new Node { Id = 7 };
            yellowPunter = new Punter { Id = 0};
            redPunter = new Punter { Id = 1 };
        }

        [Test]
        public void TestInitAndScore()
        {
            var scorer = new Scorer(new DistanceCalculator(), new GraphVisitor());
            var map = GetMapFromSampleA();

            scorer.Init(map);

            var yellowScore = scorer.Score(new GameState{Map = map, CurrentPunter = yellowPunter});
            yellowScore.Should().Be(9);

            var redScore = scorer.Score(new GameState { Map = map, CurrentPunter = redPunter });
            redScore.Should().Be(12);
        }

        private Map GetMapFromSampleA()
        {
            return new Map(
                new[] { node0, node1, node2, node3, node4, node5, node6, node7, new Node { Id = 666 } },
                new[]
                {
                    new Edge {Source = node0, Target = node1, Punter = yellowPunter},
                    new Edge {Source = node1, Target = node2, Punter = redPunter},
                    new Edge {Source = node2, Target = node3, Punter = yellowPunter},
                    new Edge {Source = node3, Target = node4, Punter = redPunter},
                    new Edge {Source = node4, Target = node5, Punter = yellowPunter},
                    new Edge {Source = node5, Target = node6, Punter = redPunter},
                    new Edge {Source = node6, Target = node7, Punter = yellowPunter},
                    new Edge {Source = node7, Target = node0, Punter = redPunter},

                    new Edge {Source = node1, Target = node3, Punter = yellowPunter},
                    new Edge {Source = node3, Target = node5, Punter = redPunter},
                    new Edge {Source = node5, Target = node7, Punter = yellowPunter},
                    new Edge {Source = node7, Target = node1, Punter = redPunter},
                }
            );
        }
    }
}