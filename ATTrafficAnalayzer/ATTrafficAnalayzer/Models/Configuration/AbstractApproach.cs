namespace ATTrafficAnalayzer.Models.Configuration
{
    abstract class AbstractApproach
    {
        /// <summary>
        ///     Name of the Abstract
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets the volume for Abstract
        /// </summary>
        /// <returns>Traffic volume</returns>
        public abstract int GetVolume();                                
    }
}
