using System;

namespace ClassmatesRPG
{
    // The OA Sorcerer - A mystical spellcaster with powerful magic
    public class TheOASorcerer : ClassFighter
    {
        private Random rand = new Random();
        private const float CRIT_CHANCE = 0.20f; // 20% chance
        private const float CRIT_MULTIPLIER = 1.5f;

        public TheOASorcerer(string name) : base(name, 120) { }

        public override int Attack()
        {
            // Magical attacks with a chance for critical hits
            int baseDamage = rand.Next(12, 25);
            bool isCritical = rand.NextDouble() < CRIT_CHANCE;
            return isCritical ? (int)(baseDamage * CRIT_MULTIPLIER) : baseDamage;
        }
    }
} 