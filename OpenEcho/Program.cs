﻿/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015  Gregory Morgan

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEcho
{
    class Program
    {
        static Quartz quartz = new Quartz();
        static QueryClassification qc = new QueryClassification();

        static void Main(string[] args)
        {
            ProcessInput();
        }

        static void ProcessInput()
        {
            Console.WriteLine("start");
            string input = Console.ReadLine().Replace("  ", " ").Trim();

            KeyValuePair<QueryClassification.Actions, string> term = qc.Classify(input);

            if (term.Key == QueryClassification.Actions.help)
            {
                Speech.say("Ask me to lookup something on wikipedia.");
            }
            else if (term.Key == QueryClassification.Actions.wikipedia)
            {
                Wikipedia wikipedia = new Wikipedia();
                input = input.Replace(term.Value, "").Trim();
                string ret = wikipedia.Search(input);
                Speech.say(ret);
            }
            else if (term.Key == QueryClassification.Actions.timer)
            {
                quartz.CreateTimer(new TimeSpan(0, 0, 5));
            }
            else if (term.Key == QueryClassification.Actions.newAction)
            {
                // usage: add search term, x to y.
                input = input.Replace(term.Value, "").Trim();
                string[] words = input.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                string verb = words[0];

                try
                {
                    QueryClassification.Actions action =
                                (QueryClassification.Actions)Enum.Parse(typeof(QueryClassification.Actions), words[1], true);
                    QueryClassification.AddVerbToAction(verb, action);
                }
                catch (ArgumentException e)
                {
                    Speech.say(e.Message);
                }
            }
        }
    }
}
