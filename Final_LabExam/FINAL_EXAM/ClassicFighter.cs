using System;

namespace ClassmatesRPG
{
    // Abstract base class for all fighters in the Classmates RPG.
    // This class defines common properties and methods for all character types.
    public abstract class ClassFighter
    {
        private string name = string.Empty;
        private int health;
        private int maxHealth;

        public string Name 
        { 
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Fighter name cannot be empty or whitespace.");
                name = value.Trim();
            }
        }

        public int MaxHealth 
        { 
            get => maxHealth;
            protected set
            {
                if (value <= 0)
                    throw new ArgumentException("Maximum health must be greater than 0.");
                maxHealth = value;
            }
        }

        public int Health 
        { 
            get => health;
            protected set
            {
                health = Math.Clamp(value, 0, MaxHealth);
            }
        }

        public ClassFighter(string name, int maxHealth)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Fighter name cannot be empty or whitespace.");
            if (maxHealth <= 0)
                throw new ArgumentException("Maximum health must be greater than 0.");

            this.Name = name.Trim();
            this.MaxHealth = maxHealth;
            this.Health = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (damage < 0)
                throw new ArgumentException("Damage cannot be negative.");
            Health = Math.Max(0, Health - damage);
        }

        public abstract int Attack();

        public virtual void Heal(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("Heal amount cannot be negative.");
            Health = Math.Min(MaxHealth, Health + amount);
        }

        public bool IsAlive => Health > 0;
    }
}

