using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Breakout
{
    public class HighScores
    {
        private string filename = "scores.xml";

        public List<ScoreRecord> scores; //  to be sorted by score highest first
          // sort using, e.g.: List<Order> SortedList = objListOrder.OrderBy(o=>o.OrderDate).ToList();

        public HighScores()
        {           
            ReadScores();
        }

        public bool IsHighScore(int score)
        {
            bool ishighscore = false;

            foreach (ScoreRecord rec in scores)
            {
                if (score > rec.score)
                {
                    ishighscore = true;
                    break;
                }
            }

            return ishighscore;
        }

        public void AddHighScore(string name, string email, int score)
        {
            scores.Add(new ScoreRecord(name, email, score));
            WriteScores();
        }

        private void ReadScores()
        {
            scores = new List<ScoreRecord>();
            try
            {
                XElement xml = XElement.Load(filename);

                bool found = false;
                scores = new List<ScoreRecord>();
                XElement HighScores = xml.XPathSelectElement("./HighScores");
                if (HighScores != null)
                {
                    foreach (XElement item in HighScores.XPathSelectElements("./HighScore"))
                    {
                        ScoreRecord rec = new ScoreRecord();
                        rec.name = (string)item.XPathSelectElement("./name");
                        rec.email = (string)item.XPathSelectElement("./email");
                        rec.score = (int)item.XPathSelectElement("./score");
                        scores.Add(rec);
                    }
                }
            }
            catch (Exception)
            {
                // uh-oh, maybe there's no file yet!
                // or it could be a corrupt file

                scores = new List<ScoreRecord>();
            }

            // fill up with blanks
            int n = scores.Count() - 1;
            if (n < 0) n = 0;
            for (int i = n; i < 10; i++)
            {
                ScoreRecord rec = new ScoreRecord("AAAAA", "", 0);
                scores.Add(rec);
            }
        }

        private void WriteScores()
        {
            // re-sort scores
            List<ScoreRecord> tmpscores 
                = scores.OrderByDescending(o=>o.score).ToList();
            scores = new List<ScoreRecord>();
            int n = 0;
            foreach (ScoreRecord rec in tmpscores)
            {
                n++;
                scores.Add(rec);
                if (n == 10) break;
            }

            try
            {
                XElement xml = new XElement("breakoutdata");

                XElement highScores = new XElement("HighScores");

                // write up to 10 elements to file! (only track 10 top scores)
                for (int i=0; i<10; i++)
                {
                    if (i < scores.Count() - 1)
                    {
                        ScoreRecord rec = scores[i];
                        highScores.Add(new XElement("HighScore",
                            new XElement("name", rec.name),
                            new XElement("email", rec.email),
                            new XElement("score", rec.score)));
                    }
                }

                xml.Add(highScores);

                xml.Save(filename);
            }
            catch (Exception)
            {
                // uh-oh
            }
        }    
    }

    public class ScoreRecord
    {
        public string name, email;
        public int score;

        public ScoreRecord(string name, string email, int score)
        {
            this.name = name;
            this.email = email;
            this.score = score;
        }

        public ScoreRecord()
        {
            this.name = "";
            this.email = "";
            this.score = 0;
        }
    }
}
