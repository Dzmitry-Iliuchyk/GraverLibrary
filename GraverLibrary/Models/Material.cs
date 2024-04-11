using System;

namespace GraverLibrary.Models
{
    public class Material
    {
        public Material()
        {
        }

        public Material(string name, byte power, byte speed, float frequency)
        {
            Name = name;
            Power = power;
            Speed = speed;
            Frequency = frequency;
        }

        /// <summary>
        /// Name of material
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Measurement in %
        /// </summary>
        /// <value>
        /// Value must be between 0 and 100
        /// </value>
        public float Power {  get; set; }
        /// <summary>
        /// Measurement in mm/sec
        /// </summary>
        /// <value>
        /// Value must be between 0 and max depends of objective (for 110x110 = 9625)
        /// </value>
        public float Speed { get; set; }
        /// <summary>
        /// Measurement in kHz
        /// </summary>
        /// <value>
        /// Value must be between 1.6 and 100
        /// </value>
        public float Frequency { get; set; }
    }
}