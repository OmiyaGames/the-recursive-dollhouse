namespace Toggler
{
    public class LeverGroup
    {
        public static System.Action<LeverGroup, bool, bool> OnBeforeStateChanged;
        static LeverGroup group = null;

        bool state = false;

        static LeverGroup Instance
        {
            get
            {
                if(group == null)
                {
                    group = new LeverGroup();
                }
                return group;
            }
        }

        public static bool IsOn
        {
            get
            {
                return Instance.state;
            }
            internal set
            {
                if (OnBeforeStateChanged != null)
                {
                    OnBeforeStateChanged(Instance, Instance.state, value);
                }
                Instance.state = value;
            }
        }
    }
}
