using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace AXE.Game.Utils
{
    class HotspotContainer
    {
        protected Dictionary<int, Vector2[]> _hotspots;
        public Dictionary<int, Vector2[]> hotspots
        {
            get 
            {
                if (_hotspots == null) 
                    _hotspots = parseFrameHotspots();
                return _hotspots; 
            }
        }

        protected string _fname;
        public string fname
        {
            get { return _fname; }
            set { _fname = value; parseFrameHotspots(); }
        }

        public HotspotContainer(string fname)
        {
            this.fname = fname;
        }

        protected Dictionary<int, Vector2[]> parseFrameHotspots()
        {
            Dictionary<int, Vector2[]> result = new Dictionary<int, Vector2[]>();

            string fname = this.fname;
            Queue<string> lines = readFile(fname);
            foreach (string line in lines)
            {
                string[] lineData = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                result.Add(int.Parse(lineData[0]),
                    new Vector2[] {
                           new Vector2(int.Parse(lineData[1]), int.Parse(lineData[2])),
                           new Vector2(int.Parse(lineData[3]), int.Parse(lineData[4]))
                    });
            }

            return result;
        }

        protected static Queue<string> readFile(string fname)
        {
            // Read cfg file
            StreamReader reader = new StreamReader(fname);
            // line by line
            Queue<String> lines = new Queue<string>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                // Remove comments and empty lines
                int index = line.IndexOf('#');
                if (line.Length <= 0 || index == 0)
                    continue;
                else if (index > 0)
                    line = line.Substring(0, index);

                // Replace tabs with spaces
                line = line.Replace('\t', ' ');
                // Remove spaces in front and after
                line = line.Trim();
                // Re-check for empty lines
                if (line.Length <= 0)
                    continue;

                lines.Enqueue(line);
            }

            reader.Close();

            return lines;
        }
    }
}
