﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using System.Threading;


namespace House
{
    class Program
    {
        public static void Main(string[] args)
        {
            SearchEng s = new SearchEng();
            string response = s.Search("how old is the oldest man");
            say(response);

            say("Hello, I am house.");
            start(null);

        }

        private static void promps(string input)
        {
            
            Weather w = new Weather();

            if (input.Contains("weather") || input.Contains("wheather"))
            {
                // look up weather
                say(w.Condition);
                say(w.Temperature);
                say(w.Hazards);
            }
            else if (input.Contains("temperature") || input.Contains("temp"))
            {
                // get just the temp
                say(w.Temperature);
            }
            else if (input.Contains("forecast"))
            {
                say(w.Forecast);
            }
            else if (input.Contains("lights"))
            {
                // turn lights on/off
                say("lights, on");
            }
            else if (input.Contains("time"))
            {
                // get the time
                say("The time is " + DateTime.Now.ToShortTimeString());
            }
            else if (input.Contains("alarm"))
            {
                // set alarm clock
            }
            else if (input.Contains("national news"))
            {
                // get the news
                say("Loading the news");
                News n = new News();
                while (true)
                {
                    if (!n.Loaded)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        break;
                    }
                }
                
                foreach (var item in n.NationalNews)
                {
                    say("");
                    say(item.Key);
                }
            }
            else if (input.Contains("local news"))
            {
                // get the news
                say("Loading the news");
                News n = new News();
                while (true)
                {
                    if (!n.Loaded)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var item in n.LocalNews)
                {
                    say("");
                    say(item.Key);
                }
            }
            else if (input.Contains("search for"))
            {
                string question = input.Replace("search for", "");
                SearchEng s = new SearchEng();
                string response = s.Search(question);
                say(response);
            }
            else
            {
                say(input + ", is not a valid command.");
            }
        }


        private static void say(string text)
        {
            // Initialize a new instance of the SpeechSynthesizer.
            SpeechSynthesizer synth = new SpeechSynthesizer();

            // Configure the audio output. 
            synth.SetOutputToDefaultAudioDevice();

            // Speak a string.
            synth.Speak(text);
        }


        private static SpeechRecognitionEngine LoadDictationGrammars()
        {

            // Create a default dictation grammar.
            DictationGrammar defaultDictationGrammar = new DictationGrammar();
            defaultDictationGrammar.Name = "default dictation";
            defaultDictationGrammar.Enabled = true;

            // Create the spelling dictation grammar.
            DictationGrammar spellingDictationGrammar =
              new DictationGrammar("grammar:dictation#spelling");
            spellingDictationGrammar.Name = "spelling dictation";
            spellingDictationGrammar.Enabled = true;

            // Create the question dictation grammar.
            DictationGrammar customDictationGrammar =
              new DictationGrammar("grammar:dictation");
            customDictationGrammar.Name = "question dictation";
            customDictationGrammar.Enabled = true;

            // Create a SpeechRecognitionEngine object and add the grammars to it.
            SpeechRecognitionEngine recoEngine = new SpeechRecognitionEngine(new CultureInfo("en-US"));
            recoEngine.LoadGrammar(defaultDictationGrammar);
            recoEngine.LoadGrammar(spellingDictationGrammar);
            recoEngine.LoadGrammar(customDictationGrammar);

            // Add a context to customDictationGrammar.
            customDictationGrammar.SetDictationContext("House, what is the", null);

            return recoEngine;
        }


        // Indicate whether asynchronous recognition is complete.
        static bool completed;

        public static void start(string[] args)
        {
            // Create an in-process speech recognizer.
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US")))
            {
                recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 50);
                recognizer.BabbleTimeout = new TimeSpan(0);
                // Create a grammar for choosing cities for a flight.
                Choices keyWords = new Choices(new string[] {"search for", "local news", "national news", "weather", "time", "temperature", "forecast", "panda" });

                GrammarBuilder gb = new GrammarBuilder();
                //gb.Append("House, what is the");
                gb.Append(keyWords);
                


                // Construct a Grammar object and load it to the recognizer.
                Grammar keywordChooser = new Grammar(gb);
                keywordChooser.Name = ("City Chooser");
                recognizer.LoadGrammarAsync(keywordChooser);

                // Attach event handlers.
                recognizer.SpeechDetected +=
                  new EventHandler<SpeechDetectedEventArgs>(
                    SpeechDetectedHandler);
                recognizer.SpeechHypothesized +=
                  new EventHandler<SpeechHypothesizedEventArgs>(
                    SpeechHypothesizedHandler);
                recognizer.SpeechRecognitionRejected +=
                  new EventHandler<SpeechRecognitionRejectedEventArgs>(
                    SpeechRecognitionRejectedHandler);
                recognizer.SpeechRecognized +=
                  new EventHandler<SpeechRecognizedEventArgs>(
                    SpeechRecognizedHandler);
                recognizer.RecognizeCompleted +=
                  new EventHandler<RecognizeCompletedEventArgs>(
                    RecognizeCompletedHandler);


                // Assign input to the recognizer and start an asynchronous
                // recognition operation.
                recognizer.SetInputToDefaultAudioDevice();
                while (true)
                {
                    completed = false;
                    //Console.WriteLine("Starting asynchronous recognition...");
                    
                    recognizer.RecognizeAsync();

                    // Wait for the operation to complete.
                    while (!completed)
                    {
                        Thread.Sleep(30);
                    }
                    Console.WriteLine("Done.");
                }
            }
        }

        // Handle the SpeechDetected event.
        static void SpeechDetectedHandler(object sender, SpeechDetectedEventArgs e)
        {
            //Console.WriteLine(" In SpeechDetectedHandler:");
            Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
        }

        // Handle the SpeechHypothesized event.
        static void SpeechHypothesizedHandler(object sender, SpeechHypothesizedEventArgs e)
        {
            //Console.WriteLine(" In SpeechHypothesizedHandler:");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            //Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
            //  grammarName, resultText);
        }

        // Handle the SpeechRecognitionRejected event.
        static void SpeechRecognitionRejectedHandler(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("RecognitionRejected: ");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                    Console.WriteLine(e.Result.Confidence);
                    say("say again");
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}", grammarName, resultText);
        }

        // Handle the SpeechRecognized event.
        static void SpeechRecognizedHandler(object sender, SpeechRecognizedEventArgs e)
        {
            //Console.WriteLine(" In SpeechRecognizedHandler.");

            string grammarName = "<not available>";
            string resultText = "<not available>";
            if (e.Result != null)
            {
                if (e.Result.Grammar != null)
                {
                    grammarName = e.Result.Grammar.Name;
                }
                resultText = e.Result.Text;
            }

            Console.WriteLine(" - Grammar Name = {0}; Result Text = {1}",
              grammarName, resultText);
        }

        // Handle the RecognizeCompleted event.
        static void RecognizeCompletedHandler(object sender, RecognizeCompletedEventArgs e)
        {
            Console.WriteLine(" In RecognizeCompletedHandler.");

            if (e.Error != null)
            {
                Console.WriteLine(
                  " - Error occurred during recognition: {0}", e.Error);
                return;
            }
            if (e.InitialSilenceTimeout || e.BabbleTimeout)
            {
                Console.WriteLine(
                  " - BabbleTimeout = {0}; InitialSilenceTimeout = {1}",
                  e.BabbleTimeout, e.InitialSilenceTimeout);
                completed = true;
                return;
            }
            if (e.InputStreamEnded)
            {
                Console.WriteLine(
                  " - AudioPosition = {0}; InputStreamEnded = {1}",
                  e.AudioPosition, e.InputStreamEnded);
            }
            if (e.Result != null)
            {
                Console.WriteLine(
                  " - Grammar = {0}; Text = {1}; Confidence = {2}",
                  e.Result.Grammar.Name, e.Result.Text, e.Result.Confidence);
                promps(e.Result.Text);
                
                //Console.WriteLine(" - AudioPosition = {0}", e.AudioPosition);
            }
            else
            {
                Console.WriteLine(" - No result.");
            }
            completed = true;
        }
    }
}
