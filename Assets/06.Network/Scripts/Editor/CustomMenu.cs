using UnityEngine;
using UnityEditor;

public class CustomMenu : MonoBehaviour
{
    // 커스텀 메뉴 항목 생성
    [MenuItem("Custom Tools/Show Game Result")]
    public static void ShowGameResult()
    {
        // PlayerPrefs에서 "GameResultDemo" 키로 string 값 불러오기
        string gameResult = PlayerPrefs.GetString("GameResultDemo", "");

        if (gameResult == "")
        {
            // 콘솔창에 출력
            Debug.Log($"No Result.");
            return;
        }

        string result = "";
        string[] lines = gameResult.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');
            if (tokens.Length != 2) continue;

            bool isMonsterWon = bool.Parse(tokens[0]);

            result += $"{(isMonsterWon ? "Monster":"Scientist")} Won, {tokens[1]} people alive.\n";
        }

        // 콘솔창에 출력
        Debug.Log(result);
    }

    // 커스텀 메뉴 항목 생성
    [MenuItem("Custom Tools/Delete Result Logs")]
    public static void DeleteResultLogs()
    {
        // PlayerPrefs에서 "GameResultDemo" 키로 string 값 불러오기
        PlayerPrefs.SetString("GameResultDemo", "");

        // 콘솔창에 출력
        Debug.Log("Result Log Is Initialized.");
    }

    // 커스텀 메뉴 항목 생성
    [MenuItem("Custom Tools/Delete Debugging Logs")]
    public static void DeleteDebugLogs()
    {
        // PlayerPrefs에서 "GameResultDemo" 키로 string 값 불러오기
        PlayerPrefs.SetInt("DebugLogIndex", 0);

        // 콘솔창에 출력
        Debug.Log("Debug Log Is Initialized.");
    }
}
