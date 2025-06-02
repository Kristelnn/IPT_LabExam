using System;

namespace ClassmatesRPG
{
    // Ante Maloi Crusader - A holy warrior with devastating divine power
    public class AnteMaloiCrusader : ClassFighter
    {
        private Random rand = new Random();
        private int consecutiveHits = 0;
        private const float EMPOWERED_MULTIPLIER = 1.8f;
        private const int HITS_FOR_EMPOWER = 3;

        public AnteMaloiCrusader(string name) : base(name, 100) { }

        public override int Attack()
        {
            // Divine attacks that grow stronger with consecutive hits
            int baseDamage = rand.Next(15, 25);
            consecutiveHits++;
            
            // Every third hit is empowered with divine energy
            if (consecutiveHits >= HITS_FOR_EMPOWER)
            {
                consecutiveHits = 0;
                return (int)(baseDamage * EMPOWERED_MULTIPLIER); // Empowered divine strike
            }
            
            return baseDamage;
        }
    }
} 