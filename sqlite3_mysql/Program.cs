using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace sqlite3_mysql
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // Get the streams
            //
            var StdOut = new StreamWriter(Console.OpenStandardOutput());
            StreamReader StdIn;
            if (args.Length == 0)
            {
                StdIn = new StreamReader(Console.OpenStandardInput());
            }
            else
            {
                StdIn = new StreamReader(args[0]);
            }

            //
            // Set MySQL to not escape backslashes in strings
            //
            StdOut.WriteLine("SET sql_mode='NO_BACKSLASH_ESCAPES';");

            //
            // Now read all text from the input and output it
            //
            // Strings can span multiple lines, so we need to keep track
            // of when we're in one for quote replacement
            //
            Char[] Buffer = new Char[1024];
            Boolean InString = false;
            List<Char> Characters = new List<Char>(1024);
            while (!StdIn.EndOfStream)
            {
                //
                // Read and process the next line
                //
                // We need to maintain line endings as lines
                // in strings can be terminated by both \r\n and
                // \n so we just look for \n then process the line
                //
                Int32 Count = StdIn.Read(Buffer, 0, Buffer.Length);
                for (Int32 i = 0; i < Count; ++i)
                {
                    //
                    // Lines can be terminated by both \r\n and \n, so look for
                    // just the '\n'
                    //
                    Characters.Add(Buffer[i]);
                    if (Buffer[i] == '\n')
                    {
                        String Line = new String(Characters.ToArray());
                        Characters.Clear();
                        ProcessLine(Line, StdOut, ref InString);
                    }
                }
            }

            //
            // Flush the output stream
            //
            StdOut.Flush();
        }

        /// <summary>
        /// Process the given line
        /// </summary>
        /// <param name="Line"></param>
        private static bool sss = true;

        private static bool in_create = false;
        private static int in_create_count = 0;
        static void ProcessLine(String Line, StreamWriter StdOut, ref Boolean InString)
        {
            if (sss)
            {
                if (Line.StartsWith("CREATE TABLE DataAircraft"))
                    sss = false;
                else return;
            }
            if (!in_create)
                if (Line.StartsWith("CREATE TABLE "))
                {
                    in_create = true;
                    in_create_count = 0;
                }
            if (Line.StartsWith("--"))
                return;

            //
            // Remove PRAGMA, BEGIN_TRANSACTION, COMMIT and sqlite_sequence lines
            //
            if (Line.StartsWith("PRAGMA") ||
                Line.StartsWith("BEGIN TRANSACTION;") ||
                Line.StartsWith("COMMIT;") ||
                Line.StartsWith("DELETE FROM sqlite_sequence;") ||
                Line.StartsWith("CREATE INDEX ") ||
                Line.StartsWith("INSERT INTO \"sqlite_sequence\""))
            {
                return;
            }
            if (Line.Contains("PRIMARY KEY"))
            {
                //StdOut.Write("\b\b"); argh
                return;
            }
            if (in_create && !(Line.StartsWith("(") || Line.StartsWith(")")))
            {
                Line = Line.Replace(",", "");

                if (in_create_count >= 2)
                    Line = "," + Line;

                in_create_count++;

            }

            if (Line.StartsWith(");"))
            {
                in_create = false;
                //Line = Line.Replace(");", ", PRIMARY KEY (ID)) ENGINE = MyISAM ;");
                Line = Line.Replace(");", ", INDEX(ID) ) ENGINE = MyISAM ;");
            }

            //
            // Replace AUTOINCREMENT with AUTO_INCREMENT
            //
            Line = Line.Replace("AUTOINCREMENT", "AUTO_INCREMENT");
            Line = Line.Replace("Range INTEGER", "MyRange INTEGER");
            //
            // Replace DEFAULT 't' and 'f' with DEFAULT '1' and '0'
            //
            Line = Line.Replace("DEFAULT 't'", "DEFAULT '1'");
            Line = Line.Replace("DEFAULT 'f'", "DEFAULT '0'");

            //
            // Now replace true and false values with 1 and 0
            //
            // We look for the , before the value to reduce the risk
            // of replacing an 't' in a string (in strings, ' should
            // be escaped with a prefix ' so we should see ''t' even
            // if it is at the end of the string
            //
            Line = Line.Replace(",'t'", ",'1'");
            Line = Line.Replace(",'f'", ",'0'");

            //
            // We now need to replace " with ` except in '
            //
            StringBuilder New = new StringBuilder(Line.Length);
            for (Int32 i = 0; i < Line.Length; ++i)
            {
                Char c = Line[i];
                if (!InString)
                {
                    if (c == '\'')
                    {
                        InString = true;
                    }
                    else if (c == '"')
                    {
                        c = '`';
                    }
                }
                else if (c == '\'')
                {
                    InString = false;
                }
                New.Append(c);
            }

            //
            // Write the line
            //
            Line = New.ToString();
            char[] ac = Line.ToCharArray();
            for (int i = 0; i < ac.Length; ++i)
            {
                if (ac[i] >= '~')
                {
                    ac[i] = '_';
                }
            }
            StdOut.Write(ac);
        }
    }
}
