namespace PureJNI.Modules.Settings
{
    public abstract class Setting
    {
        public string Name { get; }
        protected Setting(string name) => Name = name;
    }
}
