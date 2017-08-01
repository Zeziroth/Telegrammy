using System;

namespace MainWindow
{
    class Revolver
    {
        private bool[] chambers = null;

        public Revolver()
        {
            Reload();
        }

        private void Reload(int maxBullets = 1)
        {
            chambers = new bool[6];
            if (maxBullets > 6 || maxBullets < 1)
            {
                maxBullets = 1;
            }
            int curBullets = 0;

            while (curBullets < maxBullets)
            {
                int chosenChamber = Core.IntRandom(0, 6);
                bool dropBullet = Core.ResultRandom(16);

                if (dropBullet)
                {
                    chambers[chosenChamber] = true;
                    curBullets++;
                }
            }
            Spin();
        }

        internal void Spin(int rounds = -1)
        {
            rounds = rounds <= 0 ? Core.IntRandom(1, 100) : rounds;
            for (int i = 0; i < rounds; i++)
            {
                bool firstChamber = chambers[0];
                for (int j = 0; j < chambers.Length - 1; j++)
                {
                    chambers[j] = chambers[j + 1];
                }
                chambers[chambers.Length - 1] = firstChamber;
            }
        }

        internal bool Shoot()
        {
            bool hit =  chambers[0];
            chambers[0] = false;
            if (hit)
            {
                Reload();
                return true;
            }

            Spin(1);
            return false;
        }
    }
}
