using TMPro;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public const string DifficultyPrefsKey = "EnemyDifficultyIndex";

    public AIShooter shooter;
    public TMP_Dropdown dropdown;
    public string playerPrefsKey = DifficultyPrefsKey;
    [Range(0, 2)]
    public int defaultDifficultyIndex = 1;

    void Awake()
    {
        SetupDropdownOptions();
    }

    void OnEnable()
    {
        if (dropdown == null)
        {
            return;
        }

        dropdown.onValueChanged.AddListener(SetDifficultyFromDropdown);

        int savedIndex = PlayerPrefs.GetInt(playerPrefsKey, defaultDifficultyIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, 2);
        dropdown.SetValueWithoutNotify(savedIndex);
        SetDifficultyFromDropdown(savedIndex);
    }

    void OnDisable()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(SetDifficultyFromDropdown);
        }
    }

    void SetupDropdownOptions()
    {
        if (dropdown != null && dropdown.options.Count == 0)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData("0%"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("50%"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("100%"));
        }
    }

    public void SetDifficultyFromDropdown(int index)
    {
        index = Mathf.Clamp(index, 0, 2);
        PlayerPrefs.SetInt(playerPrefsKey, index);
        PlayerPrefs.Save();

        if (shooter != null)
        {
            shooter.SetDifficulty(GetDifficultyPercent(index));
        }
    }

    public static float GetSavedDifficultyPercent(float fallbackPercent)
    {
        if (!PlayerPrefs.HasKey(DifficultyPrefsKey))
        {
            return Mathf.Clamp01(fallbackPercent);
        }

        return GetDifficultyPercent(PlayerPrefs.GetInt(DifficultyPrefsKey, 1));
    }

    static float GetDifficultyPercent(int index)
    {
        return index switch
        {
            0 => 0f,
            1 => 0.5f,
            2 => 1f,
            _ => 0f
        };
    }
}
