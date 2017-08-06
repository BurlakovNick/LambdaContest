using System.Collections.Generic;
using System.Linq;
using Core.Objects;

namespace Core.Components
{
    public class ComponentScore
    {
        public List<Node> Mines { get; set; }
        public Dictionary<int, long> Scores { get; set; }
        public long SelfScore { get; set; }

        public ComponentScore(List<Node> mines, Dictionary<int, long> scores)
        {
            Mines = mines;
            Scores = scores;
            SelfScore = Mines.Sum(x => Scores[x.Id]);
        }

        public ComponentScore(Node node, Node[] mines, IScorer scorer)
        {
            Mines = new[] { node }.Where(x => x.IsMine).ToList();
            Scores = new Dictionary<int, long>();
            foreach (var mine in mines)
            {
                var d = scorer.GetDistance(mine, node);
                Scores.Add(mine.Id, d * d);
            }
            SelfScore = Mines.Sum(x => Scores[x.Id]);
        }

        public ComponentScore Add(ComponentScore other)
        {
            Mines.AddRange(other.Mines);
            foreach (var score in other.Scores)
            {
                Scores[score.Key] += score.Value;
            }
            SelfScore = Mines.Sum(x => Scores[x.Id]);
            return this;
        }

        public ComponentScore Clone()
        {
            return new ComponentScore(Mines.Select(x => x).ToList(), Scores.Select(x => x).ToDictionary(x => x.Key, x => x.Value));
        }
    }
}