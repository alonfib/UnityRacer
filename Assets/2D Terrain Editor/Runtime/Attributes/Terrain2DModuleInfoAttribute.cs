using System;

namespace T2D.Modules
{
    /// <summary>
    /// <see cref="Terrain2DModule"/> info attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Terrain2DModuleInfoAttribute : Attribute
    {
        /// <summary>
        /// <see cref="Terrain2DModule"/> name
        /// </summary>
        public string Name;

        /// <summary>
        /// Determines whether an <see cref="Terrain2DModule"/> component should be attached to other object than <see cref="Terrain2D"/> is attached to
        /// </summary>
        public bool NewGameObjectRequired;

        /// <summary>
        /// Determines whether an <see cref="Terrain2DModule"/> can have multiple instances on the same game object
        /// </summary>
        public bool AllowMultipleInstances;


        public Terrain2DModuleInfoAttribute(string name, bool newGameObjectRequired = false, bool allowMultipleInstances = false)
        {
            Name = name;
            NewGameObjectRequired = newGameObjectRequired;
            AllowMultipleInstances = allowMultipleInstances;
        }
    }
}