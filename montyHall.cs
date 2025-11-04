using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace tutorial
{
    class Program
    {
        static void Main(string[] args)
        {
			bool playing = true;
			while (playing)
            {
				Console.WriteLine("How many times do you want to play? ");
				int numPlays = Convert.ToInt32( Console.ReadLine());
				Monty_Hall(numPlays);
                Console.WriteLine("Do you want to play again? Y or N");
				char play_again = Convert.ToChar( Console.ReadLine());
				if (char.ToUpper(play_again) == 'N') { playing = false; }
            }
			

            //Console.ReadLine();
        }

        static void Monty_Hall(double plays)
        {

			//const double plays = 10000;

			double wins = 0;
			double losses = 0;
			Random rnd = new Random();

			for (int i = 0; i <= plays; i++)
			{
				string[] doors = { "goat", "goat", "goat" };
				int prize = rnd.Next(0, 3);
				doors[prize] = "prize";
				int guess = rnd.Next(0, 3);
				if (doors[guess] == "prize")
				{
					losses++;
				}
				else
				{
					wins++;
				}


				// Console.WriteLine("prize and guess are: " + prize + " " + guess);
			}

			double winPercent = (wins / plays) * 100;
			double losePercent = (losses / plays) * 100;

			Console.WriteLine("Wins is " + winPercent + " percent. and Losses is " + losePercent + " percent");

			//Console.ReadLine();
		}
       

    }
}