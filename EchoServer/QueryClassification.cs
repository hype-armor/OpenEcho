﻿/*
    OpenEcho is a program to automate basic tasks at home all while being handsfree.
    Copyright (C) 2015 Gregory Morgan

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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Extensions;

namespace EchoServer
{
    class QueryClassification
    {
        private Dictionary<string, HashSet<string>> actionDatabase = new Dictionary<string, HashSet<string>>();

        void QueryClassificationf()
        {
            AddPhraseToAction("help", help);
        }

        public void AddPhraseToAction(string phrase, string _action)
        {
            if (!actionDatabase.Keys.Contains(_action))
            {
                actionDatabase.Add(_action, new HashSet<string>());
            }

            phrase = phrase.CleanText();

            HashSet<string> phrases = actionDatabase[_action];
            phrases.Add(phrase);
            actionDatabase[_action] = phrases;

        }

        public KeyValuePair<string, string> Classify(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new KeyValuePair<string, string>("blank", "");
            }

            Dictionary<string, string> matchedSubjects = new Dictionary<string, string>();

            foreach (KeyValuePair<string, HashSet<string>> item in actionDatabase)
            {
                string phrase = item.Key;
                HashSet<string> subjects = item.Value;

                foreach (string subject in subjects)
                {
                    if (input.Contains(subject) && !matchedSubjects.Keys.Contains(phrase))
                    {
                        matchedSubjects.Add(phrase, subject);
                    }
                    else if (input.Contains(subject) && matchedSubjects.Keys.Contains(phrase) && matchedSubjects[phrase].Length < subject.Length)
                    {
                        matchedSubjects[phrase] = subject;
                    }
                }
            }

            if (matchedSubjects.Count() == 1)
            {
                return matchedSubjects.First();
            }
            else if (matchedSubjects.Count() > 1)
            {
                return new KeyValuePair<string, string>
                    ("unknown", "There is more than one match for your query. Please remove one of the matches from my database. The matched query was " + matchedSubjects.First().Key);
            }
            else
            {
                return new KeyValuePair<string, string>("unknown", "I cannot match your query to anything in my database.");
            }
        }

        public string help
        {
            get
            {
                StringBuilder helpText = new StringBuilder();
                foreach (var term in actionDatabase)
                {
                    foreach (var value in term.Value)
                    {
                        helpText.AppendLine(term.Key + ": " + value);
                    }
                }

                return helpText.ToString();
            }
        }
    }
}
