using System;

namespace ClassmatesRPG
{
    // The Tao Lang Adventurer - A skilled warrior with unpredictable combat techniques
    public class TaoLangAdventurer : ClassFighter
    {
        private Random rand = new Random();
        private int lastDamage = 0;
        private const int QUICK_STRIKE_MIN = 10;
        private const int QUICK_STRIKE_MAX = 15;
        private const int POWER_STRIKE_MIN = 18;
        private const int POWER_STRIKE_MAX = 30;
        private const int DAMAGE_THRESHOLD = 20;

        public TaoLangAdventurer(string name) : base(name, 110) { }

        public override int Attack()
        {
            // Unpredictable attack pattern that alternates between quick and powerful strikes
            int minDamage = lastDamage > DAMAGE_THRESHOLD ? QUICK_STRIKE_MIN : POWER_STRIKE_MIN;
            int maxDamage = lastDamage > DAMAGE_THRESHOLD ? QUICK_STRIKE_MAX : POWER_STRIKE_MAX;
            
            lastDamage = rand.Next(minDamage, maxDamage + 1);
            return lastDamage;
        }
    }
} 