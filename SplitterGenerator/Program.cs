using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitterGenerator
{
    class Program
    {
        static bool PromptForLoop()
        {
            Console.Write("Create another splitter setup? [Y/n] ");
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.N: return false;
                default: return true;
            }
        }
        static int PromptForInt(string prompt)
        {
            while (true)
            {
                try
                {
                    Console.Write(prompt + " ");
                    string response = Console.ReadLine();
                    int parsedResponse = int.Parse(response);
                    return parsedResponse;
                }
                catch
                {
                    Console.Write("Invalid input. ");
                }
            }
        }

        static void Main(string[] args)
        {
            do
            {
                Console.Clear();

            input:

                // Get the number of output belts
                int NumberOfOutputs = PromptForInt("How many output belts should there be?");
                Console.WriteLine();

                // Get the ratio for each output belt
                List<int> OutputRatios = new List<int>();
                for (int i = 0; i < NumberOfOutputs; i++)
                {
                    OutputRatios.Add(PromptForInt("Ratio for Belt " + (i + 1) + ":"));
                }

                // Reduce the ratios based on their GCD
                int factor = Util.GCD(OutputRatios);
                for (int i = 0; i < OutputRatios.Count; i++)
                {
                    OutputRatios[i] /= factor;
                }

                // Find how many splitter levels will be required for the ratios given
                int TotalRatioValue = OutputRatios.Sum();
                int SplitterLevels = 0;
                while (Math.Pow(2, SplitterLevels) < TotalRatioValue) SplitterLevels++;

                // Generate list of flags for final splitter row
                List<int> flags = new List<int>();
                for (int i = 0; i < OutputRatios.Count; i++)
                    for (int j = 0; j < OutputRatios[i]; j++)
                        flags.Add(i + 1);

                Splitter SplitterTree = new Splitter(SplitterLevels);
                SplitterTree.FlagEndNodes(flags);

                Console.WriteLine();

                // Condense nodes of splitter tree
                Splitter CondenseNodes(Splitter splitter)
                {
                    if (splitter.HasOutputs)
                    {

                        splitter.Outputs[0] = CondenseNodes(splitter.Outputs[0]);
                        splitter.Outputs[1] = CondenseNodes(splitter.Outputs[1]);

                        if (splitter.Outputs[0].Flag == splitter.Outputs[1].Flag)
                        {
                            splitter.Flag = splitter.Outputs[0].Flag;
                            if (splitter.Flag != 0)
                            {
                                splitter.Outputs[0].Hidden = true;
                                splitter.Outputs[1].Hidden = true;
                            }
                        }
                    }
                    return splitter;
                }

                for (int i = 0; i < SplitterLevels; i++)
                    SplitterTree = CondenseNodes(SplitterTree);

                // Print splitter tree
                List<string> PrintLevels = new List<string>();
                void PrintTree(Splitter splitter, int lvl)
                {
                    if (lvl == PrintLevels.Count) PrintLevels.Add("");

                    string midSpace = "";
                    for (int i = 0; i < Math.Pow(2, SplitterLevels - lvl + 1) - 1; i++) midSpace += " ";

                    string splitterString = "";

                    if (splitter.Flag >= 10 && splitter.Flag < 35)
                        splitterString = Convert.ToChar((byte)(55 + splitter.Flag)).ToString();
                    else if (splitter.Flag >= 35)
                        splitterString = Convert.ToChar((byte)(62 + splitter.Flag)).ToString();
                    else splitterString = splitter.Flag.ToString();

                    PrintLevels[lvl] += (splitter.Hidden ? "." : splitter.Flag != -1 ? splitterString : "X") + midSpace;

                    if (splitter.HasOutputs)
                    {
                        PrintTree(splitter.Outputs[0], lvl + 1);
                        PrintTree(splitter.Outputs[1], lvl + 1);
                    }
                }
                PrintTree(SplitterTree, 0);

                for (int i = 1; i < PrintLevels.Count; i++)
                {
                    string startingSpace = "";
                    for (int j = 0; j < Math.Pow(2, i) - 1; j++) startingSpace += " ";

                    int index = PrintLevels.Count - 1 - i;
                    PrintLevels[index] = startingSpace + PrintLevels[index];
                }

                Console.WriteLine();
                foreach (string level in PrintLevels) Console.WriteLine(level);
                Console.WriteLine();
            }
            while (PromptForLoop());
        }
    }

    class Util
    {
        public static int GCD(List<int> numbers) => numbers.Aggregate(GCD);
        private static int GCD(int x, int y) => y == 0 ? x : GCD(y, x % y);
    }

    class Splitter
    {
        #region Public Fields
        public Splitter[] Outputs { get; set; } = new Splitter[2] { null, null };
        public int Flag { get; set; } = 0;
        public bool Hidden { get; set; } = false;
        public bool HasOutputs
        {
            get
            {
                return Outputs[0] != null && Outputs[1] != null;
            }
        }
        #endregion

        #region Constructor
        public Splitter(int levels)
        {
            if (levels != 0)
                for (int i = 0; i < 2; i++)
                    Outputs[i] = new Splitter(levels - 1);
        }
        #endregion

        public void FlagEndNodes(List<int> flags) => FlagEndNodes(0, SortFlags(flags));
        private int FlagEndNodes(int counter, List<int> flags)
        {
            if (!HasOutputs)
            {
                if (counter < flags.Count) Flag = flags[counter];
                else Flag = -1;
                counter++;
            }
            else
            {
                counter = Outputs[0].FlagEndNodes(counter, flags);
                counter = Outputs[1].FlagEndNodes(counter, flags);
            }

            return counter;
        }

        private Dictionary<int, int> buildFlagHistogram(List<int> flags)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();

            foreach (var flag in flags)
            {
                if (dict.ContainsKey(flag))
                    dict[flag]++;
                else dict[flag] = 1;
            }

            return dict;

        }

        private List<int> SortFlags(List<int> flags)
        {

            Dictionary<int, int> flagHistogram = buildFlagHistogram(flags);
            List<int> sortedFlags = new List<int>();
            List<int> odds = new List<int>();
            List<int> evens = new List<int>();

            foreach (var flag in flagHistogram.Keys)
            {
                for (int i = 0; i < flagHistogram[flag]; ++i)
                    if (flagHistogram[flag] % 2 == 0)
                        evens.Add(flag);
                    else
                        odds.Add(flag);
            }

            odds.Sort();
            evens.Sort();


            sortedFlags.AddRange(evens);
            sortedFlags.AddRange(odds);

            return sortedFlags;
        }
    }
}
