using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excercise
{
    class Program
    {
        static void Main(string[] args)
        {
            // THREAD SYNC
            //Console.WriteLine("*****Synchronizing Threads *****\n");
            //Printer p = new Printer();
            //// Make 10 threads that are all pointing to the same
            //// method on the same object.
            //Thread[] threads = new Thread[10];

            //for (int i = 0; i < 10; i++)
            //{
            //    threads[i] = new Thread(new ThreadStart(p.PrintNumbers))
            //    {
            //        Name = $"Worker thread #{i}"
            //    };
            //}

            //// Now start each one.
            //foreach (Thread t in threads)
            //    t.Start();


            // THREAD INFO
            // Prompt user for a PID and print out the set of active threads.
            //Console.WriteLine("***** Enter PID of process to investigate *****");
            //Console.Write("PID: ");
            //string pID = Console.ReadLine();
            //int theProcID = int.Parse(pID);
            //EnumThreadsForPid(theProcID);

            // START PROCESS
            StartAndKillProcess();

            Console.ReadLine();

            
        }

        static void EnumThreadsForPid(int pID)
        {
            Process theProc = null;
            try
            {
                theProc = Process.GetProcessById(pID);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            // List out stats for each thread in the specified process.
            Console.WriteLine("Here are the threads used by: {0}", theProc.ProcessName);
            ProcessThreadCollection theThreads = theProc.Threads;
            foreach (ProcessThread pt in theThreads)
            {
                string info =
                    $"-> Thread ID: {pt.Id}\tStart Time: {pt.StartTime.ToShortTimeString()}\tPriority:{ pt.PriorityLevel}";
                Console.WriteLine(info);
            }
            Console.WriteLine("************************************\n");
        }

        static void StartAndKillProcess()
        {
            Process ffProc = null;
            // Launch Firefox, and go to Facebook!
            try
            {
                ffProc = Process.Start("chrome.exe", "www.facebook.com");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Write("--> Hit enter to kill {0}...", ffProc.ProcessName);
            Console.ReadLine();
            // Kill the iexplore.exe process.
            try
            {
                ffProc.Kill();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class Printer
    {
        private static object threadLock = new object();

        public void PrintNumbers()
        {
            // Use the private object lock token.
            //lock (threadLock)
            { 
                // Display Thread info.
                Console.WriteLine("-> {0} is executing PrintNumbers()",
                Thread.CurrentThread.Name);
                // Print out numbers.
                Console.Write("Your numbers: ");
                for (int i = 0; i < 10; i++)
                {
                    Random r = new Random();
                    Thread.Sleep(10 * r.Next(5));
                    Console.Write("{0}, ", i);
                }
                Console.WriteLine();
            }
        }
    }
}
