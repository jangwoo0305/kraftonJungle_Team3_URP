using System;
using System.IO;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class DynamoDBManager : MonoBehaviour
{
    private static AmazonDynamoDBClient client;

    string filePath = "";

    public async Task LoadData(string token, PlayerData playerData)
    {
        InitializeDB();


        // UserToken이 빈 문자열일 경우 새로운 UserToken과 랜덤 닉네임 생성 및 저장
        if (string.IsNullOrEmpty(token))
        {
            string newUserToken = GenerateHashKey(); // 새로운 UserToken 생성
            string randomNickname = GenerateRandomNickname(); // 랜덤 닉네임 생성

            playerData.UserToken = newUserToken;
            playerData.Nickname = randomNickname;

            // 생성한 PlayerData를 데이터베이스에 저장
            await SavePlayerData(playerData);
            await ConnectPlayer(playerData);
        }
        else
        {
            // UserToken이 비어있지 않은 경우 기존 플레이어 데이터에 연결
            playerData.UserToken = token;

            await ConnectPlayer(playerData); // 기존 데이터와 연결
        }
    }


    public void InitializeDB()
    {
        if (client == null)
        {
            TextAsset envFile = Resources.Load<TextAsset>("AWS"); // "AWS.env"에서 ".env"는 생략
            if (envFile == null)
            {
                Debug.LogError("AWS credentials file not found in Resources folder.");
                return;
            }
            string[] lines = envFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


            string accessKey = lines[0];
            string secretKey = lines[1];
            string region = lines[2];


            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.GetBySystemName(region));
        }
    }


    public async Task SavePlayerData(PlayerData playerData)
    {
        var request = new PutItemRequest
        {
            TableName = "PlayerData",
            Item = new Dictionary<string, AttributeValue>
            {
                { "UserToken", new AttributeValue { S = playerData.UserToken } },
                { "Nickname", new AttributeValue { S = playerData.Nickname } }
            }
        };

        try
        {
            var response = await client.PutItemAsync(request);
            Debug.Log("Player data saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save player data: " + e.Message);
        }
    }

    public async Task<Dictionary<string, AttributeValue>> GetPlayerDataByUserToken(string UserToken)
    {
        var request = new GetItemRequest
        {
            TableName = "PlayerData",
            Key = new Dictionary<string, AttributeValue>
            {
                { "UserToken", new AttributeValue { S = UserToken } }
            }
        };

        try
        {
            var response = await client.GetItemAsync(request);
            if (response.Item != null && response.Item.Count > 0)
            {
                return response.Item;
            }
            else
            {
                Debug.Log("No player data found for UserToken.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve player data by UserToken: " + e.Message);
            return null;
        }
    }

    public async Task ConnectPlayer(PlayerData playerData)
    {
        var existingPlayerData = await GetPlayerDataByUserToken(playerData.UserToken);
        if (existingPlayerData == null)
        {
            string randomNickname = GenerateRandomNickname(); // 랜덤 닉네임 생성

            playerData.UserToken = playerData.UserToken;
            playerData.Nickname = randomNickname;

            // 생성한 PlayerData를 데이터베이스에 저장
            await SavePlayerData(playerData);
        }
        else
        {
            playerData.Nickname = existingPlayerData["Nickname"].S;
        }
    }

    
    // 고유한 키를 생성하는 해시 함수
    private string GenerateHashKey()
    {
        return $"KEY_{DateTime.UtcNow.Ticks}";
    }

    // 랜덤 닉네임을 생성하는 함수
    private string GenerateRandomNickname()
    {
        DateTime now = DateTime.UtcNow;
        // 현재 시간의 틱 수를 기반으로 고유한 닉네임 생성
        return $"Player_{now.Hour:D2}_{now.Minute:D2}_{now.Second:D2}";
    }

    // 닉네임을 업데이트하는 메서드
    public async Task UpdateNickname(string token, string newName)
    {
        var request = new UpdateItemRequest
        {
            TableName = "PlayerData",
            Key = new Dictionary<string, AttributeValue>
            {
                { "UserToken", new AttributeValue { S = token } }
            },
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#N", "Nickname" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":newName", new AttributeValue { S = newName } }
            },
            UpdateExpression = "SET #N = :newName"
        };

        try
        {
            var response = await client.UpdateItemAsync(request);
            Debug.Log("Nickname updated successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to update nickname: " + e.Message);
        }
    }


    public async Task DeletePlayerDataByToken(string UserToken)
    {
        // UserToken을 통해 PlayerData 조회
        var existingPlayerData = await GetPlayerDataByUserToken(UserToken);

        if (existingPlayerData != null)
        {
            // PlayerData가 존재하는 경우, 삭제 요청을 실행합니다.
            var request = new DeleteItemRequest
            {
                TableName = "PlayerData",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "UserToken", new AttributeValue { S = UserToken } }
                }
            };

            try
            {
                // 삭제 요청 실행
                var response = await client.DeleteItemAsync(request);
                Debug.Log($"Player data with UserToken {UserToken} deleted successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete player data: " + e.Message);
            }
        }
        else
        {
            Debug.Log("No player data found for the provided UserToken. Deletion skipped.");
        }
    }
}
