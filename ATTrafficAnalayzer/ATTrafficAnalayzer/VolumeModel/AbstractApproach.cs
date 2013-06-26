namespace ATTrafficAnalayzer.VolumeModel
{
    abstract class AbstractApproach
    {
        public string Name { get; set; }
        public abstract int GetVolume();
    }
}
