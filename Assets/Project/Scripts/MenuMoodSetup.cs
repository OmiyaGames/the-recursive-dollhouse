using UnityEngine;

public class MenuMoodSetup : MonoBehaviour
{
    [System.Serializable]
    public struct DollhouseSet
    {
        [SerializeField]
        MeshRenderer dollhouse;
        [SerializeField]
        MeshRenderer table;

        public MeshRenderer Dollhouse
        {
            get
            {
                return dollhouse;
            }
        }

        public MeshRenderer Table
        {
            get
            {
                return table;
            }
        }
    }

    [SerializeField]
    MoodSetter mood;
    [SerializeField]
    DollhouseSet[] allDollhouses;

    // Use this for initialization
    void Start()
    {
        if (allDollhouses.Length > 0)
        {
            // Setup the first dollhouse
            int index = 0;
            MoodTheme newTheme = mood.RandomTheme;
            DollHouse.UpdateTheme(allDollhouses[index].Dollhouse, mood, newTheme);
            allDollhouses[index].Table.sharedMaterial = newTheme.WallMaterial;
            mood.SnapToTheme(newTheme);

            // Setup the rest
            ++index;
            for (; index < allDollhouses.Length; ++index)
            {
                newTheme = mood.RandomTheme;
                DollHouse.UpdateTheme(allDollhouses[index].Dollhouse, mood, newTheme);
                allDollhouses[index].Table.sharedMaterial = newTheme.WallMaterial;
            }

        }
    }
}
