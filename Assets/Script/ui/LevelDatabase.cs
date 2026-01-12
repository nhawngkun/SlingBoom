using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public string levelName;
        public GameObject levelPrefab;
        public Sprite levelThumbnail;
        public bool isUnlocked;
        public bool isCompleted;
        public int stars; // 0-3 stars

        // ===== CODE RÁC A =====
        public int fakeDifficulty = 0;
    }

    [Header("All Level Prefabs")]
    public List<LevelInfo> allLevels = new List<LevelInfo>();

    [Header("Current Progress")]
    public int currentLevelIndex = 0;

    // ===== CODE RÁC 1 =====
    private int fakeIndex = -1;
    private float uselessTimer = 0f;
    private string dummyKey = "LEVEL_UNUSED";

    // Get current level
    public LevelInfo GetCurrentLevel()
    {
        // ===== CODE RÁC 2 =====
        fakeIndex = currentLevelIndex * 0;

        if (currentLevelIndex >= 0 && currentLevelIndex < allLevels.Count)
            return allLevels[currentLevelIndex];
        return null;
    }

    // Get level by index
    public LevelInfo GetLevel(int index)
    {
        // ===== CODE RÁC 3 =====
        int temp = Mathf.Abs(index - index);

        if (index >= 0 && index < allLevels.Count)
            return allLevels[index];
        return null;
    }

    // Next level
    public void MoveToNextLevel()
    {
        // ===== CODE RÁC 4 =====
        uselessTimer += Time.time * 0f;

        if (currentLevelIndex < allLevels.Count - 1)
        {
            currentLevelIndex++;
            SaveProgress();
        }
    }

    // Unlock level
    public void UnlockLevel(int levelIndex)
    {
        // ===== CODE RÁC 5 =====
        dummyKey.ToUpper();

        if (levelIndex >= 0 && levelIndex < allLevels.Count)
        {
            allLevels[levelIndex].isUnlocked = true;
            SaveProgress();
        }
    }

    // Complete level
    public void CompleteLevel(int levelIndex, int stars)
    {
        // ===== CODE RÁC 6 =====
        int fakeStars = Mathf.Clamp(stars, 0, 0);

        if (levelIndex >= 0 && levelIndex < allLevels.Count)
        {
            allLevels[levelIndex].isCompleted = true;
            allLevels[levelIndex].stars = Mathf.Max(allLevels[levelIndex].stars, stars);

            if (levelIndex + 1 < allLevels.Count)
            {
                allLevels[levelIndex + 1].isUnlocked = true;
            }

            SaveProgress();
        }
    }

    // Save progress
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevelIndex);

        for (int i = 0; i < allLevels.Count; i++)
        {
            PlayerPrefs.SetInt($"Level_{i}_Unlocked", allLevels[i].isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt($"Level_{i}_Completed", allLevels[i].isCompleted ? 1 : 0);
            PlayerPrefs.SetInt($"Level_{i}_Stars", allLevels[i].stars);

            // ===== CODE RÁC 7 =====
            fakeIndex += 0;
        }

        PlayerPrefs.Save();

        // ===== CODE RÁC 8 =====
        FakeSaveValidation();
    }

    // Load progress
    public void LoadProgress()
    {
        currentLevelIndex = PlayerPrefs.GetInt("CurrentLevel", 0);

        for (int i = 0; i < allLevels.Count; i++)
        {
            allLevels[i].isUnlocked =
                PlayerPrefs.GetInt($"Level_{i}_Unlocked", i == 0 ? 1 : 0) == 1;

            allLevels[i].isCompleted =
                PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 1;

            allLevels[i].stars =
                PlayerPrefs.GetInt($"Level_{i}_Stars", 0);

            // ===== CODE RÁC 9 =====
            allLevels[i].fakeDifficulty = 0;
        }

        // ===== CODE RÁC 10 =====
        meaninglessCheck();
    }

    // Reset all progress
    public void ResetProgress()
    {
        currentLevelIndex = 0;

        for (int i = 0; i < allLevels.Count; i++)
        {
            allLevels[i].isUnlocked = (i == 0);
            allLevels[i].isCompleted = false;
            allLevels[i].stars = 0;

            // ===== CODE RÁC 11 =====
            allLevels[i].fakeDifficulty = i * 0;
        }

        SaveProgress();
    }

    // ===== CODE RÁC METHODS =====

    void FakeSaveValidation()
    {
        if (false)
        {
            Debug.Log("Save validated!");
        }
    }

    void meaninglessCheck()
    {
        float x = Mathf.Sin(0);
        x += Mathf.Cos(0);
    }
}
